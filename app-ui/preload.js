const { contextBridge, ipcRenderer } = require('electron/renderer')

contextBridge.exposeInMainWorld('appApi', {
    onReloadClicked: (callback) => ipcRenderer.on('reload-clicked', (_event) => callback()),
    performReload: () => ipcRenderer.send('perform-reload'),
    performAppQuit: () => ipcRenderer.send('perform-app-quit'),
    openUrlExternal: (protocolUrl) => ipcRenderer.send('open-url-external', protocolUrl),
    resolveFileUrl: (filename) => ipcRenderer.send('resolve-file-url', filename),
    onResolveFileUrl: (callback) => ipcRenderer.on('resolve-file-url-result', (_event, resolvedUrl) => callback(resolvedUrl)),
    showMessageBox: (options) => ipcRenderer.send('show-message-box', options),
    onShowMessageBox: (callback) => ipcRenderer.on('show-message-box-result', (_event, result) => callback(result)),
    showSaveDialog: (options) => ipcRenderer.send('show-save-dialog', options),
    onShowSaveDialog: (callback) => ipcRenderer.on('show-save-dialog-result', (_event, result) => callback(result)),
    showOpenDialog: (options) => ipcRenderer.send('show-open-dialog', options),
    onShowOpenDialog: (callback) => ipcRenderer.on('show-open-dialog-result', (_event, result) => callback(result)),
    getApiUrl: () => ipcRenderer.send('get-api-url'),
    onGetApiUrl: (callback) => ipcRenderer.on('get-api-url-result', (_event, url) => callback(url)),
})
