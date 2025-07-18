import { NavigateFunction } from 'react-router-dom';

let navigate: NavigateFunction | null = null;

export const setNavigate = (navFunc: NavigateFunction) => {
  navigate = navFunc;
};

export const getNavigate = () => navigate;