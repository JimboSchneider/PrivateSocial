import { useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { setNavigate } from '../services/navigationHelper';

export default function NavigationSetup({ children }: { children: React.ReactNode }) {
  const navigate = useNavigate();

  useEffect(() => {
    setNavigate(navigate);
  }, [navigate]);

  return <>{children}</>;
}