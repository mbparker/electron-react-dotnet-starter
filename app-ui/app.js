const {app, BrowserWindow, ipcMain, dialog, shell, Menu} = require('electron')
const url = require("url");
const path = require("path");
const cProcess = require('child_process').spawn
const fs = require('fs');
const os = require('os');

// PROJECT SPECIFIC CONSTANTS
const APP_API_PORT = 5149;
const APP_NAME = 'ElectronApp';
const APP_API_NAME = `${APP_NAME}ApiHost`;
//

process.env.ASPNETCORE_URLS = `http://localhost:${APP_API_PORT}`;

function getDateString() {
  const date = new Date();
  const year = date.getFullYear();
  const month = `${date.getMonth() + 1}`.padStart(2, '0');
  const day =`${date.getDate()}`.padStart(2, '0');
  return `${APP_NAME}_App_NodeMain_${year}-${month}-${day}`
}

let localAppData;
if (os.platform() === 'win32') {
  localAppData = 'AppData/Local';
} else {
  localAppData = '.local/share';
}

let logFilename = `${os.homedir()}/${localAppData}/${APP_API_NAME}/Logs/${getDateString()}.log`;
if (!fs.existsSync(path.dirname(logFilename))) {
  fs.mkdirSync(path.dirname(logFilename), {recursive: true});
}

let originalConsole = console;
let logStream = fs.createWriteStream(logFilename, { flags: 'a', autoClose: true });
console = new console.Console(logStream, logStream, true);

let splashWindow;
let mainWindow;
const contentPath = __dirname.replace('app.asar', '');
const currentPreloadPath = path.join(contentPath, 'preload.js');
const currentApiPath = path.join(contentPath, 'api');
const currentUiPath = __dirname;
let apiProcess;

ipcMain.on('show-message-box', async (evt, ...args) => {
  const result = await dialog.showMessageBox(mainWindow, args[0]);
  evt.sender.send('show-message-box-result', result);
});

ipcMain.on('show-open-dialog', async (evt, ...args) => {
  const result = await dialog.showOpenDialog(mainWindow, args[0]);
  evt.sender.send('show-open-dialog-result', result);
});

ipcMain.on('show-save-dialog', async (evt, ...args) => {
  const result = await dialog.showSaveDialog(mainWindow, args[0]);
  evt.sender.send('show-save-dialog-result', result);
});

ipcMain.on('resolve-file-url', (evt, ...args) => {
  try {
    const result = url.pathToFileURL(path.join(currentUiPath, args[0])).toString();
    evt.sender.send('resolve-file-url-result', result);
  } catch (err) {
    console.error(err);
    evt.sender.send('resolve-file-url-result', args[0]);
  }
});

ipcMain.on('open-url-external', async (evt, ...args) => {
  await shell.openExternal(args[0], { activate: true });
});

ipcMain.on('perform-reload', (evt, ...args) => {
  mainWindow?.reload();
});

ipcMain.on('perform-app-quit', (evt, ...args) => {
  app.quit();
});

const menuTemplate = [
  {
    label: 'File',
    submenu: [
      {
        label: 'Reload',
        click: () => {
          mainWindow?.webContents.send('reload-clicked');
        }
      },
      { type: 'separator' },
      { role: 'quit' }
    ]
  },
  {
    label: 'View',
    submenu: [
      { role: 'toggleDevTools' },
      { type: 'separator' },
      { role: 'resetZoom' },
      { role: 'zoomIn' },
      { role: 'zoomOut' },
      { type: 'separator' },
      { role: 'togglefullscreen' }
    ]
  }
];

const menu = Menu.buildFromTemplate(menuTemplate);
Menu.setApplicationMenu(menu);

function createSplashWindow() {

  splashWindow = new BrowserWindow({
    width: 626,
    height: 626,
    transparent: true,
    center: true,
    frame: false,
    closable: false,
    resizable: false,
    skipTaskbar: true,
    alwaysOnTop: true,
    show: true
  });
  splashWindow.setIgnoreMouseEvents(true);
  let targetUrl = url.pathToFileURL(path.join(currentUiPath, 'splash.html'));
  splashWindow.loadURL(targetUrl.toString()).then(() => {
    console.log('splash screen loaded');
  }, (err) => {
    console.log('failed to load splash screen');
    console.error(err);
  });
  splashWindow.on('closed', () => {
    splashWindow = null
  });

}

function createWindow () {
  mainWindow = new BrowserWindow({
    width: 1280,
    height: 1024,
    backgroundColor: '#2a2a2a',
    webPreferences: {
      nodeIntegration: true,
      contextIsolation: true,
      webSecurity: true,
      preload: currentPreloadPath,
    }
  })

  let targetUrl = url.pathToFileURL(path.join(currentUiPath, 'index.html'));
  mainWindow.loadURL(targetUrl.toString()).then(() => {
    // Open the DevTools.
    //mainWindow.webContents.openDevTools();
    splashWindow?.destroy();
  });

  mainWindow.on('closed', () => {
    mainWindow = null
  })
}

function startApi() {
  let binaryFile = APP_API_NAME;

  if (os.platform() === 'win32') {
    binaryFile = binaryFile + '.exe';
  }

  let binFilePath = path.join(currentApiPath, binaryFile);
  let options = { cwd: currentApiPath };
  let parameters = [];
  console.log(`Launching .NET Backend: ${binFilePath}`);
  try {
    apiProcess = cProcess(binFilePath, parameters, options);

    if (apiProcess) {
      apiProcess.stdout.on('data', (data) => {
        console.log(`stdout: ${data.toString()}`);
      });
    } else {
      console.warn('Failed to create API process.');
    }
  } catch (err) {
    console.error(err);
  }
}

app.on('ready', () => {
  createSplashWindow();
  startApi();
  setTimeout(() => {
    createWindow();
  }, 2000);
})

app.on('window-all-closed', () => {
  //if (process.platform !== 'darwin')
    app.quit();
})

app.on('activate', () => {
  if (mainWindow === null) {
    createWindow();
  }
})

app.on('quit', (event, exitCode) => {
  try {
    console.info('Terminating API process...');
    if (apiProcess) {
      if (!apiProcess.kill()) {
        console.warn('Failed to terminate API process.');
      } else {
        console.warn('API process terminated. Hasta la vista.');
      }
    } else {
      console.warn('API process was never created.');
    }
  } finally {
    this.console = originalConsole;
  }
})
