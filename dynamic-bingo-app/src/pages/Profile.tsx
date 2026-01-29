import React, { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { Button } from '@/components/ui/button';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';
import { AlertDialog, AlertDialogAction, AlertDialogCancel, AlertDialogContent, AlertDialogDescription, AlertDialogFooter, AlertDialogHeader, AlertDialogTitle, AlertDialogTrigger } from '@/components/ui/alert-dialog';
import { ArrowLeft, Save, Trash2, Mail } from 'lucide-react';
import { useAuthStore } from '../stores/authStore';
import { apiService } from '../services/api';

export function Profile() {
  const navigate = useNavigate();
  const { user, setUser, logout } = useAuthStore();
  const [isLoading, setIsLoading] = useState(false);
  const [profile, setProfile] = useState({
    displayName: user?.displayName || '',
    email: user?.email || ''
  });
  const [convertEmail, setConvertEmail] = useState('');
  const [showConvertSuccess, setShowConvertSuccess] = useState(false);

  const handleUpdateProfile = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!user) return;

    setIsLoading(true);
    try {
      const updatedUser = await apiService.updateProfile({
        displayName: profile.displayName,
        email: profile.email
      });
      setUser(updatedUser);
    } catch (error) {
      console.error('Error updating profile:', error);
    } finally {
      setIsLoading(false);
    }
  };

  const handleDeleteAccount = async () => {
    if (!user) return;

    try {
      await apiService.deleteAccount();
      logout();
      navigate('/');
    } catch (error) {
      console.error('Error deleting account:', error);
    }
  };

  const handleConvertGuest = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!convertEmail.trim()) return;

    setIsLoading(true);
    try {
      await apiService.convertGuest(convertEmail);
      setShowConvertSuccess(true);
      setConvertEmail('');
    } catch (error) {
      console.error('Error converting guest account:', error);
    } finally {
      setIsLoading(false);
    }
  };

  if (!user) {
    navigate('/');
    return null;
  }

  if (showConvertSuccess) {
    return (
      <div className="min-h-screen bg-gradient-to-br from-blue-50 to-purple-50 flex items-center justify-center p-4">
        <Card className="w-full max-w-md">
          <CardHeader className="text-center">
            <div className="mx-auto w-12 h-12 bg-green-100 rounded-full flex items-center justify-center mb-4">
              <Mail className="w-6 h-6 text-green-600" />
            </div>
            <CardTitle>Conversion Link Sent</CardTitle>
          </CardHeader>
          <CardContent className="text-center space-y-4">
            <p className="text-gray-600">
              We've sent a magic link to <strong>{convertEmail}</strong>
            </p>
            <p className="text-sm text-gray-500">
              Click the link in your email to convert your guest account to a registered account.
            </p>
            <Button onClick={() => navigate('/lobby')} className="w-full">
              Return to Lobby
            </Button>
          </CardContent>
        </Card>
      </div>
    );
  }

  return (
    <div className="min-h-screen bg-gradient-to-br from-blue-50 to-purple-50 p-4">
      <div className="max-w-2xl mx-auto">
        <div className="flex items-center gap-4 mb-8">
          <Button variant="outline" onClick={() => navigate('/lobby')}>
            <ArrowLeft className="w-4 h-4 mr-2" />
            Back to Lobby
          </Button>
          <div>
            <h1 className="text-3xl font-bold text-gray-800">Profile Settings</h1>
            <p className="text-gray-600">Manage your account information</p>
          </div>
        </div>

        <div className="space-y-6">
          {user.isGuest && (
            <Card className="border-blue-200 bg-blue-50">
              <CardHeader>
                <CardTitle className="text-blue-800">Convert Guest Account</CardTitle>
              </CardHeader>
              <CardContent>
                <p className="text-blue-700 mb-4">
                  Convert your guest account to a registered account to keep your progress and avoid automatic deletion.
                </p>
                <form onSubmit={handleConvertGuest} className="space-y-4">
                  <div>
                    <Label htmlFor="convertEmail">Email Address</Label>
                    <Input
                      id="convertEmail"
                      type="email"
                      value={convertEmail}
                      onChange={(e) => setConvertEmail(e.target.value)}
                      placeholder="Enter your email"
                      required
                    />
                  </div>
                  <Button type="submit" disabled={isLoading} className="w-full">
                    {isLoading ? 'Sending...' : 'Send Conversion Link'}
                  </Button>
                </form>
              </CardContent>
            </Card>
          )}

          <Card>
            <CardHeader>
              <CardTitle>Account Information</CardTitle>
            </CardHeader>
            <CardContent>
              <form onSubmit={handleUpdateProfile} className="space-y-4">
                <div>
                  <Label htmlFor="displayName">Display Name</Label>
                  <Input
                    id="displayName"
                    value={profile.displayName}
                    onChange={(e) => setProfile(prev => ({ ...prev, displayName: e.target.value }))}
                    placeholder="Enter display name"
                    maxLength={30}
                    required
                  />
                  <p className="text-xs text-gray-500 mt-1">
                    1-30 characters. Allowed: letters, numbers, _ - $ @ ^ . ( ) [ ] {'{}'} !
                  </p>
                </div>

                {!user.isGuest && (
                  <div>
                    <Label htmlFor="email">Email Address</Label>
                    <Input
                      id="email"
                      type="email"
                      value={profile.email}
                      onChange={(e) => setProfile(prev => ({ ...prev, email: e.target.value }))}
                      placeholder="Enter email address"
                      required
                    />
                  </div>
                )}

                <Button type="submit" disabled={isLoading} className="w-full">
                  <Save className="w-4 h-4 mr-2" />
                  {isLoading ? 'Saving...' : 'Save Changes'}
                </Button>
              </form>
            </CardContent>
          </Card>

          <Card className="border-red-200">
            <CardHeader>
              <CardTitle className="text-red-800">Danger Zone</CardTitle>
            </CardHeader>
            <CardContent>
              <p className="text-gray-600 mb-4">
                Permanently delete your account and all associated data. This action cannot be undone.
              </p>
              <AlertDialog>
                <AlertDialogTrigger asChild>
                  <Button variant="destructive">
                    <Trash2 className="w-4 h-4 mr-2" />
                    Delete Account
                  </Button>
                </AlertDialogTrigger>
                <AlertDialogContent>
                  <AlertDialogHeader>
                    <AlertDialogTitle>Are you absolutely sure?</AlertDialogTitle>
                    <AlertDialogDescription>
                      This action cannot be undone. This will permanently delete your account
                      and remove all your data from our servers. Your game history will be preserved
                      but your account will be marked as deleted.
                    </AlertDialogDescription>
                  </AlertDialogHeader>
                  <AlertDialogFooter>
                    <AlertDialogCancel>Cancel</AlertDialogCancel>
                    <AlertDialogAction onClick={handleDeleteAccount} className="bg-red-600 hover:bg-red-700">
                      Delete Account
                    </AlertDialogAction>
                  </AlertDialogFooter>
                </AlertDialogContent>
              </AlertDialog>
            </CardContent>
          </Card>
        </div>
      </div>
    </div>
  );
}
