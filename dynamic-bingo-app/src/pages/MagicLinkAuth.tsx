import React, { useEffect, useState } from 'react';
import { useNavigate, useSearchParams } from 'react-router-dom';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Button } from '@/components/ui/button';
import { Loader2, CheckCircle, XCircle } from 'lucide-react';
import { apiService } from '../services/api';
import { useAuthStore } from '../stores/authStore';

export function MagicLinkAuth() {
  const [searchParams] = useSearchParams();
  const navigate = useNavigate();
  const { setUser } = useAuthStore();
  const [status, setStatus] = useState<'loading' | 'success' | 'error'>('loading');
  const [error, setError] = useState<string>('');

  useEffect(() => {
    const token = searchParams.get('token');
    
    if (!token) {
      setStatus('error');
      setError('Invalid or missing token');
      return;
    }

    const consumeToken = async () => {
      try {
        const response = await apiService.consumeMagicLink(token);
        localStorage.setItem('auth-token', response.accessToken);
        
        const user = await apiService.getMe();
        setUser(user);
        setStatus('success');
        
        setTimeout(() => {
          navigate('/lobby');
        }, 2000);
      } catch (error) {
        console.error('Error consuming magic link:', error);
        setStatus('error');
        setError('Invalid or expired token');
      }
    };

    consumeToken();
  }, [searchParams, setUser, navigate]);

  const handleReturnHome = () => {
    navigate('/');
  };

  return (
    <div className="min-h-screen bg-gradient-to-br from-blue-50 to-purple-50 flex items-center justify-center p-4">
      <Card className="w-full max-w-md">
        <CardHeader className="text-center">
          <div className="mx-auto w-12 h-12 rounded-full flex items-center justify-center mb-4">
            {status === 'loading' && (
              <div className="bg-blue-100">
                <Loader2 className="w-6 h-6 text-blue-600 animate-spin" />
              </div>
            )}
            {status === 'success' && (
              <div className="bg-green-100">
                <CheckCircle className="w-6 h-6 text-green-600" />
              </div>
            )}
            {status === 'error' && (
              <div className="bg-red-100">
                <XCircle className="w-6 h-6 text-red-600" />
              </div>
            )}
          </div>
          <CardTitle>
            {status === 'loading' && 'Signing you in...'}
            {status === 'success' && 'Welcome back!'}
            {status === 'error' && 'Authentication Failed'}
          </CardTitle>
        </CardHeader>
        <CardContent className="text-center space-y-4">
          {status === 'loading' && (
            <p className="text-gray-600">
              Please wait while we verify your magic link...
            </p>
          )}
          {status === 'success' && (
            <p className="text-gray-600">
              You've been successfully signed in. Redirecting to the lobby...
            </p>
          )}
          {status === 'error' && (
            <>
              <p className="text-gray-600">{error}</p>
              <Button onClick={handleReturnHome} className="w-full">
                Return to Home
              </Button>
            </>
          )}
        </CardContent>
      </Card>
    </div>
  );
}
