import React, { useEffect, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { Button } from '@/components/ui/button';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Input } from '@/components/ui/input';
import { Badge } from '@/components/ui/badge';
import { Tabs, TabsContent, TabsList, TabsTrigger } from '@/components/ui/tabs';
import { ArrowLeft, UserPlus, Check, X, Users } from 'lucide-react';
import { useAuthStore } from '../stores/authStore';
import { useLobbyStore } from '../stores/lobbyStore';
import { signalRService } from '../services/signalr';
import { apiService } from '../services/api';

export function Friends() {
  const navigate = useNavigate();
  const { user } = useAuthStore();
  const { friends, onlineUsers, presence } = useLobbyStore();
  const [searchEmail, setSearchEmail] = useState('');
  const [isLoading, setIsLoading] = useState(false);

  useEffect(() => {
    const initializeFriends = async () => {
      try {
        const friendsList = await apiService.getFriends();
        useLobbyStore.getState().setFriends(friendsList);
      } catch (error) {
        console.error('Error loading friends:', error);
      }
    };

    initializeFriends();
  }, []);

  const handleSendFriendRequest = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!searchEmail.trim()) return;

    setIsLoading(true);
    try {
      const targetUser = onlineUsers.find(u => u.email === searchEmail.trim());
      if (targetUser) {
        await signalRService.sendFriendRequest(targetUser.id);
        setSearchEmail('');
      }
    } catch (error) {
      console.error('Error sending friend request:', error);
    } finally {
      setIsLoading(false);
    }
  };

  const handleRespondToRequest = async (requestId: string, accept: boolean) => {
    try {
      await signalRService.respondFriendRequest(requestId, accept);
    } catch (error) {
      console.error('Error responding to friend request:', error);
    }
  };

  const getPresenceStatus = (userId: string) => {
    const userPresence = presence[userId];
    return userPresence?.status || 'Available';
  };

  const getPresenceBadgeVariant = (status: string) => {
    switch (status) {
      case 'Available': return 'default';
      case 'InGame': return 'secondary';
      case 'Busy': return 'destructive';
      default: return 'outline';
    }
  };

  const pendingRequests = friends.filter(f => f.status === 'Pending');
  const sentRequests = pendingRequests.filter(f => f.userAId === user?.id);
  const receivedRequests = pendingRequests.filter(f => f.userBId === user?.id);
  const acceptedFriends = friends.filter(f => f.status === 'Accepted');

  if (!user) {
    navigate('/');
    return null;
  }

  return (
    <div className="min-h-screen bg-gradient-to-br from-blue-50 to-purple-50 p-4">
      <div className="max-w-4xl mx-auto">
        <div className="flex items-center gap-4 mb-8">
          <Button variant="outline" onClick={() => navigate('/lobby')}>
            <ArrowLeft className="w-4 h-4 mr-2" />
            Back to Lobby
          </Button>
          <div>
            <h1 className="text-3xl font-bold text-gray-800">Friends</h1>
            <p className="text-gray-600">Manage your friend connections</p>
          </div>
        </div>

        <div className="space-y-6">
          {/* Add Friend */}
          <Card>
            <CardHeader>
              <CardTitle className="flex items-center gap-2">
                <UserPlus className="w-5 h-5" />
                Add Friend
              </CardTitle>
            </CardHeader>
            <CardContent>
              <form onSubmit={handleSendFriendRequest} className="flex gap-2">
                <Input
                  type="email"
                  placeholder="Enter friend's email address"
                  value={searchEmail}
                  onChange={(e) => setSearchEmail(e.target.value)}
                  className="flex-1"
                />
                <Button type="submit" disabled={isLoading}>
                  {isLoading ? 'Sending...' : 'Send Request'}
                </Button>
              </form>
            </CardContent>
          </Card>

          {/* Friends Tabs */}
          <Card>
            <CardHeader>
              <CardTitle className="flex items-center gap-2">
                <Users className="w-5 h-5" />
                Your Friends ({acceptedFriends.length})
              </CardTitle>
            </CardHeader>
            <CardContent>
              <Tabs defaultValue="friends">
                <TabsList className="grid w-full grid-cols-3">
                  <TabsTrigger value="friends">
                    Friends ({acceptedFriends.length})
                  </TabsTrigger>
                  <TabsTrigger value="received">
                    Requests ({receivedRequests.length})
                  </TabsTrigger>
                  <TabsTrigger value="sent">
                    Sent ({sentRequests.length})
                  </TabsTrigger>
                </TabsList>

                <TabsContent value="friends" className="space-y-3 mt-4">
                  {acceptedFriends.length === 0 ? (
                    <p className="text-gray-500 text-center py-8">
                      No friends yet. Send some friend requests to get started!
                    </p>
                  ) : (
                    acceptedFriends.map((friendship) => {
                      const friend = friendship.userAId === user.id ? friendship.userB : friendship.userA;
                      const isOnline = onlineUsers.some(u => u.id === friend.id);
                      
                      return (
                        <div key={friendship.id} className="flex items-center justify-between p-3 border rounded-lg">
                          <div className="flex items-center gap-3">
                            <div className={`w-3 h-3 rounded-full ${isOnline ? 'bg-green-500' : 'bg-gray-400'}`} />
                            <div>
                              <p className="font-medium">{friend.displayName}</p>
                              <p className="text-sm text-gray-600">{friend.email}</p>
                            </div>
                          </div>
                          <div className="flex items-center gap-2">
                            {isOnline && (
                              <Badge variant={getPresenceBadgeVariant(getPresenceStatus(friend.id))}>
                                {getPresenceStatus(friend.id)}
                              </Badge>
                            )}
                            <span className="text-sm text-gray-500">
                              Friends since {new Date(friendship.acceptedAt || friendship.createdAt).toLocaleDateString()}
                            </span>
                          </div>
                        </div>
                      );
                    })
                  )}
                </TabsContent>

                <TabsContent value="received" className="space-y-3 mt-4">
                  {receivedRequests.length === 0 ? (
                    <p className="text-gray-500 text-center py-8">
                      No pending friend requests
                    </p>
                  ) : (
                    receivedRequests.map((request) => (
                      <div key={request.id} className="flex items-center justify-between p-3 border rounded-lg bg-blue-50">
                        <div>
                          <p className="font-medium">{request.userA.displayName}</p>
                          <p className="text-sm text-gray-600">{request.userA.email}</p>
                          <p className="text-xs text-gray-500">
                            Sent {new Date(request.createdAt).toLocaleDateString()}
                          </p>
                        </div>
                        <div className="flex gap-2">
                          <Button
                            size="sm"
                            onClick={() => handleRespondToRequest(request.id, true)}
                          >
                            <Check className="w-4 h-4 mr-1" />
                            Accept
                          </Button>
                          <Button
                            size="sm"
                            variant="outline"
                            onClick={() => handleRespondToRequest(request.id, false)}
                          >
                            <X className="w-4 h-4 mr-1" />
                            Decline
                          </Button>
                        </div>
                      </div>
                    ))
                  )}
                </TabsContent>

                <TabsContent value="sent" className="space-y-3 mt-4">
                  {sentRequests.length === 0 ? (
                    <p className="text-gray-500 text-center py-8">
                      No pending sent requests
                    </p>
                  ) : (
                    sentRequests.map((request) => (
                      <div key={request.id} className="flex items-center justify-between p-3 border rounded-lg">
                        <div>
                          <p className="font-medium">{request.userB.displayName}</p>
                          <p className="text-sm text-gray-600">{request.userB.email}</p>
                          <p className="text-xs text-gray-500">
                            Sent {new Date(request.createdAt).toLocaleDateString()}
                          </p>
                        </div>
                        <Badge variant="outline">Pending</Badge>
                      </div>
                    ))
                  )}
                </TabsContent>
              </Tabs>
            </CardContent>
          </Card>
        </div>
      </div>
    </div>
  );
}
