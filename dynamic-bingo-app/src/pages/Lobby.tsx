import React, { useEffect, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { Button } from '@/components/ui/button';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';
import { Badge } from '@/components/ui/badge';
import { Dialog, DialogContent, DialogHeader, DialogTitle, DialogTrigger } from '@/components/ui/dialog';
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '@/components/ui/select';
import { Tabs, TabsContent, TabsList, TabsTrigger } from '@/components/ui/tabs';
import { Users, Plus, Play, UserPlus, Settings, LogOut } from 'lucide-react';
import { useAuthStore } from '../stores/authStore';
import { useLobbyStore } from '../stores/lobbyStore';
import { signalRService } from '../services/signalr';
import { apiService } from '../services/api';

export function Lobby() {
  const navigate = useNavigate();
  const { user, logout } = useAuthStore();
  const { onlineUsers, openChallenges, friends, presence } = useLobbyStore();
  const [isCreateChallengeOpen, setIsCreateChallengeOpen] = useState(false);
  const [joinCode, setJoinCode] = useState('');
  const [challengeForm, setChallengeForm] = useState({
    word: '',
    visibility: 'Public',
    fillMode: 'Random',
    starterChoice: 'Random'
  });

  useEffect(() => {
    const initializeLobby = async () => {
      try {
        await signalRService.connectToLobby();
        
        const [users, challenges, friendsList] = await Promise.all([
          apiService.getLobbyUsers(),
          apiService.getOngoingGames(),
          apiService.getFriends()
        ]);
        
        useLobbyStore.getState().setOnlineUsers(users);
        useLobbyStore.getState().setFriends(friendsList);
      } catch (error) {
        console.error('Error initializing lobby:', error);
      }
    };

    initializeLobby();

    return () => {
      signalRService.disconnect();
    };
  }, []);

  const handleCreateChallenge = async () => {
    if (!challengeForm.word.trim() || challengeForm.word.length < 4 || challengeForm.word.length > 8) {
      return;
    }

    try {
      await signalRService.createOpenChallenge(challengeForm);
      setIsCreateChallengeOpen(false);
      setChallengeForm({
        word: '',
        visibility: 'Public',
        fillMode: 'Random',
        starterChoice: 'Random'
      });
    } catch (error) {
      console.error('Error creating challenge:', error);
    }
  };

  const handleAcceptChallenge = async (challengeId: string) => {
    try {
      const result = await signalRService.acceptChallenge(challengeId);
      navigate(`/game/${result.gameId}`);
    } catch (error) {
      console.error('Error accepting challenge:', error);
    }
  };

  const handleJoinByCode = () => {
    if (joinCode.trim()) {
      navigate(`/game/${joinCode.trim()}`);
    }
  };

  const handleSendFriendRequest = async (targetUserId: string) => {
    try {
      await signalRService.sendFriendRequest(targetUserId);
    } catch (error) {
      console.error('Error sending friend request:', error);
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

  const isFriend = (userId: string) => {
    return friends.some(f => 
      (f.userAId === userId || f.userBId === userId) && 
      f.status === 'Accepted'
    );
  };

  if (!user) {
    navigate('/');
    return null;
  }

  return (
    <div className="min-h-screen bg-gradient-to-br from-blue-50 to-purple-50 p-4">
      <div className="max-w-7xl mx-auto">
        <div className="flex justify-between items-center mb-8">
          <div>
            <h1 className="text-4xl font-bold text-gray-800">Dynamic Bingo Lobby</h1>
            <p className="text-gray-600">Welcome back, {user.displayName}!</p>
          </div>
          <div className="flex gap-2">
            <Button variant="outline" onClick={() => navigate('/profile')}>
              <Settings className="w-4 h-4 mr-2" />
              Profile
            </Button>
            <Button variant="outline" onClick={logout}>
              <LogOut className="w-4 h-4 mr-2" />
              Logout
            </Button>
          </div>
        </div>

        <div className="grid lg:grid-cols-3 gap-6">
          <div className="lg:col-span-2 space-y-6">
            <Card>
              <CardHeader>
                <CardTitle className="flex items-center gap-2">
                  <Play className="w-5 h-5" />
                  Quick Actions
                </CardTitle>
              </CardHeader>
              <CardContent className="space-y-4">
                <div className="flex gap-4">
                  <Dialog open={isCreateChallengeOpen} onOpenChange={setIsCreateChallengeOpen}>
                    <DialogTrigger asChild>
                      <Button className="flex-1">
                        <Plus className="w-4 h-4 mr-2" />
                        Create Challenge
                      </Button>
                    </DialogTrigger>
                    <DialogContent>
                      <DialogHeader>
                        <DialogTitle>Create New Challenge</DialogTitle>
                      </DialogHeader>
                      <div className="space-y-4">
                        <div>
                          <Label htmlFor="word">Word (4-8 letters)</Label>
                          <Input
                            id="word"
                            value={challengeForm.word}
                            onChange={(e) => setChallengeForm(prev => ({ ...prev, word: e.target.value }))}
                            placeholder="Enter word for the game"
                            maxLength={8}
                          />
                        </div>
                        <div>
                          <Label>Visibility</Label>
                          <Select value={challengeForm.visibility} onValueChange={(value) => setChallengeForm(prev => ({ ...prev, visibility: value }))}>
                            <SelectTrigger>
                              <SelectValue />
                            </SelectTrigger>
                            <SelectContent>
                              <SelectItem value="Public">Public</SelectItem>
                              <SelectItem value="Friends">Friends Only</SelectItem>
                              <SelectItem value="Private">Private</SelectItem>
                            </SelectContent>
                          </Select>
                        </div>
                        <div>
                          <Label>Fill Mode</Label>
                          <Select value={challengeForm.fillMode} onValueChange={(value) => setChallengeForm(prev => ({ ...prev, fillMode: value }))}>
                            <SelectTrigger>
                              <SelectValue />
                            </SelectTrigger>
                            <SelectContent>
                              <SelectItem value="Sequential">Sequential</SelectItem>
                              <SelectItem value="Random">Random</SelectItem>
                              <SelectItem value="Manual">Manual</SelectItem>
                            </SelectContent>
                          </Select>
                        </div>
                        <div>
                          <Label>Who Starts</Label>
                          <Select value={challengeForm.starterChoice} onValueChange={(value) => setChallengeForm(prev => ({ ...prev, starterChoice: value }))}>
                            <SelectTrigger>
                              <SelectValue />
                            </SelectTrigger>
                            <SelectContent>
                              <SelectItem value="Creator">I Start</SelectItem>
                              <SelectItem value="Opponent">Opponent Starts</SelectItem>
                              <SelectItem value="Random">Random</SelectItem>
                            </SelectContent>
                          </Select>
                        </div>
                        <Button onClick={handleCreateChallenge} className="w-full">
                          Create Challenge
                        </Button>
                      </div>
                    </DialogContent>
                  </Dialog>
                </div>
                
                <div className="flex gap-2">
                  <Input
                    placeholder="Enter game code"
                    value={joinCode}
                    onChange={(e) => setJoinCode(e.target.value)}
                    className="flex-1"
                  />
                  <Button onClick={handleJoinByCode} variant="outline">
                    Join Game
                  </Button>
                </div>
              </CardContent>
            </Card>

            <Card>
              <CardHeader>
                <CardTitle>Open Challenges</CardTitle>
              </CardHeader>
              <CardContent>
                {openChallenges.length === 0 ? (
                  <p className="text-gray-500 text-center py-8">No open challenges available</p>
                ) : (
                  <div className="space-y-3">
                    {openChallenges.map((challenge) => (
                      <div key={challenge.id} className="flex items-center justify-between p-3 border rounded-lg">
                        <div className="flex-1">
                          <div className="flex items-center gap-2">
                            <span className="font-medium">{challenge.creator.displayName}</span>
                            <Badge variant="outline">{challenge.visibility}</Badge>
                            <Badge variant="secondary">{challenge.fillMode}</Badge>
                          </div>
                          <p className="text-sm text-gray-600">
                            Word: {challenge.word} • {challenge.word.length}×{challenge.word.length} board
                          </p>
                        </div>
                        <Button 
                          onClick={() => handleAcceptChallenge(challenge.id)}
                          disabled={challenge.creatorId === user.id}
                        >
                          Accept
                        </Button>
                      </div>
                    ))}
                  </div>
                )}
              </CardContent>
            </Card>
          </div>

          <div className="space-y-6">
            <Card>
              <CardHeader>
                <CardTitle className="flex items-center gap-2">
                  <Users className="w-5 h-5" />
                  Online Users ({onlineUsers.length})
                </CardTitle>
              </CardHeader>
              <CardContent>
                <Tabs defaultValue="all">
                  <TabsList className="grid w-full grid-cols-2">
                    <TabsTrigger value="all">All</TabsTrigger>
                    <TabsTrigger value="friends">Friends</TabsTrigger>
                  </TabsList>
                  
                  <TabsContent value="all" className="space-y-2 mt-4">
                    {onlineUsers.map((onlineUser) => (
                      <div key={onlineUser.id} className="flex items-center justify-between p-2 border rounded">
                        <div className="flex items-center gap-2">
                          <div className="flex flex-col">
                            <span className="text-sm font-medium">{onlineUser.displayName}</span>
                            <Badge variant={getPresenceBadgeVariant(getPresenceStatus(onlineUser.id))} className="text-xs w-fit">
                              {getPresenceStatus(onlineUser.id)}
                            </Badge>
                          </div>
                        </div>
                        {onlineUser.id !== user.id && !isFriend(onlineUser.id) && (
                          <Button 
                            size="sm" 
                            variant="outline"
                            onClick={() => handleSendFriendRequest(onlineUser.id)}
                          >
                            <UserPlus className="w-3 h-3" />
                          </Button>
                        )}
                      </div>
                    ))}
                  </TabsContent>
                  
                  <TabsContent value="friends" className="space-y-2 mt-4">
                    {friends
                      .filter(f => f.status === 'Accepted')
                      .map((friendship) => {
                        const friend = friendship.userAId === user.id ? friendship.userB : friendship.userA;
                        return (
                          <div key={friendship.id} className="flex items-center justify-between p-2 border rounded">
                            <div className="flex items-center gap-2">
                              <div className="flex flex-col">
                                <span className="text-sm font-medium">{friend.displayName}</span>
                                <Badge variant={getPresenceBadgeVariant(getPresenceStatus(friend.id))} className="text-xs w-fit">
                                  {getPresenceStatus(friend.id)}
                                </Badge>
                              </div>
                            </div>
                          </div>
                        );
                      })}
                  </TabsContent>
                </Tabs>
              </CardContent>
            </Card>
          </div>
        </div>
      </div>
    </div>
  );
}
