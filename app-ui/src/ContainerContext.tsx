import React, { useContext } from "react";
import { container, DependencyContainer, InjectionToken } from "tsyringe";

const ContainerContext = React.createContext<DependencyContainer>(container);

export const useService = <T extends unknown>(token: InjectionToken<T>) => {
    const container = useContext(ContainerContext);
    return container.resolve(token) as T;
};

export default ContainerContext;
