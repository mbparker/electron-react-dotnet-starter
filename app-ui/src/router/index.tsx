import { useRoutes } from "react-router-dom";
import Layout from "../layouts/Layout";
import Home from "../pages/Home";

function Router() {
  const routes = [
    {
      path: "/",
      element: <Layout />,
      children: [
        {
          path: "/",
          element: <Home />,
        },
      ],
    },
  ];

  return useRoutes(routes);
}

export default Router;
