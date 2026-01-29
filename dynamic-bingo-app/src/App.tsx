import React from 'react';
import { BrowserRouter as Router, Routes, Route, Navigate } from 'react-router-dom';
import { useAuthStore } from './stores/authStore';
import { Landing } from './pages/Landing';
import { MagicLinkAuth } from './pages/MagicLinkAuth';
import { Lobby } from './pages/Lobby';
import { Profile } from './pages/Profile';
import { GameRoom } from './pages/GameRoom';
import { Friends } from './pages/Friends';
import { OngoingGames } from './pages/OngoingGames';
import './App.css';

function ProtectedRoute({ children }: { children: React.ReactNode }) {
  const { isAuthenticated } = useAuthStore();
  return isAuthenticated ? <>{children}</> : <Navigate to="/" replace />;
}

function App() {
  // Match Vite base so client-side routes work on GitHub Pages (e.g. /repo-name/)
  const basename = import.meta.env.BASE_URL.replace(/\/$/, "") || undefined;
  return (
    <Router basename={basename}>
      <Routes>
        <Route path="/" element={<Landing />} />
        <Route path="/auth" element={<MagicLinkAuth />} />
        <Route 
          path="/lobby" 
          element={
            <ProtectedRoute>
              <Lobby />
            </ProtectedRoute>
          } 
        />
        <Route 
          path="/profile" 
          element={
            <ProtectedRoute>
              <Profile />
            </ProtectedRoute>
          } 
        />
        <Route 
          path="/friends" 
          element={
            <ProtectedRoute>
              <Friends />
            </ProtectedRoute>
          } 
        />
        <Route 
          path="/games" 
          element={
            <ProtectedRoute>
              <OngoingGames />
            </ProtectedRoute>
          } 
        />
        <Route 
          path="/game/:gameId" 
          element={
            <ProtectedRoute>
              <GameRoom />
            </ProtectedRoute>
          } 
        />
        <Route path="*" element={<Navigate to="/" replace />} />
      </Routes>
    </Router>
  );
}

export default App;
