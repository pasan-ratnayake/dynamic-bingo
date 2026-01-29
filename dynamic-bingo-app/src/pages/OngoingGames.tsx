import { useEffect, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { Button } from '@/components/ui/button';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Badge } from '@/components/ui/badge';
import { ArrowLeft, Play, Clock, Trophy } from 'lucide-react';
import { useAuthStore } from '../stores/authStore';
import { apiService } from '../services/api';

interface OngoingGame {
  id: string;
  word: string;
  status: string;
  createdAt: string;
  startedAt?: string;
  creatorId: string;
  opponentId?: string;
  creator: {
    id: string;
    displayName: string;
  };
  opponent?: {
    id: string;
    displayName: string;
  };
  myScore: number;
  opponentScore: number;
}

export function OngoingGames() {
  const navigate = useNavigate();
  const { user } = useAuthStore();
  const [games, setGames] = useState<OngoingGame[]>([]);
  const [isLoading, setIsLoading] = useState(true);

  useEffect(() => {
    const loadOngoingGames = async () => {
      try {
        const ongoingGames = await apiService.getOngoingGames();
        setGames(ongoingGames);
      } catch (error) {
        console.error('Error loading ongoing games:', error);
      } finally {
        setIsLoading(false);
      }
    };

    loadOngoingGames();
  }, []);

  const handleJoinGame = (gameId: string) => {
    navigate(`/game/${gameId}`);
  };

  const getGameStatusBadge = (status: string) => {
    switch (status) {
      case 'Pending': return { variant: 'outline' as const, text: 'Waiting for opponent' };
      case 'Active': return { variant: 'default' as const, text: 'In progress' };
      case 'Finished': return { variant: 'secondary' as const, text: 'Finished' };
      case 'Forfeited': return { variant: 'destructive' as const, text: 'Forfeited' };
      case 'Draw': return { variant: 'secondary' as const, text: 'Draw' };
      default: return { variant: 'outline' as const, text: status };
    }
  };

  const getOpponentName = (game: OngoingGame) => {
    if (!game.opponent) return 'Waiting for opponent...';
    return game.creatorId === user?.id ? game.opponent.displayName : game.creator.displayName;
  };

  const getMyRole = (game: OngoingGame) => {
    return game.creatorId === user?.id ? 'Creator' : 'Opponent';
  };

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
            <h1 className="text-3xl font-bold text-gray-800">Ongoing Games</h1>
            <p className="text-gray-600">Resume your active games</p>
          </div>
        </div>

        <Card>
          <CardHeader>
            <CardTitle className="flex items-center gap-2">
              <Play className="w-5 h-5" />
              Your Games ({games.length})
            </CardTitle>
          </CardHeader>
          <CardContent>
            {isLoading ? (
              <div className="text-center py-8">
                <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-blue-600 mx-auto mb-4"></div>
                <p className="text-gray-600">Loading your games...</p>
              </div>
            ) : games.length === 0 ? (
              <div className="text-center py-8">
                <p className="text-gray-500 mb-4">No ongoing games found</p>
                <Button onClick={() => navigate('/lobby')}>
                  Go to Lobby
                </Button>
              </div>
            ) : (
              <div className="space-y-4">
                {games.map((game) => {
                  const statusBadge = getGameStatusBadge(game.status);
                  const opponentName = getOpponentName(game);
                  const myRole = getMyRole(game);
                  
                  return (
                    <div key={game.id} className="border rounded-lg p-4 hover:bg-gray-50 transition-colors">
                      <div className="flex items-center justify-between">
                        <div className="flex-1">
                          <div className="flex items-center gap-3 mb-2">
                            <h3 className="text-lg font-semibold">
                              {game.word.toUpperCase()} Bingo
                            </h3>
                            <Badge variant={statusBadge.variant}>
                              {statusBadge.text}
                            </Badge>
                            <Badge variant="outline">
                              {myRole}
                            </Badge>
                          </div>
                          
                          <div className="grid grid-cols-1 md:grid-cols-3 gap-4 text-sm text-gray-600">
                            <div className="flex items-center gap-2">
                              <Clock className="w-4 h-4" />
                              <span>
                                {game.startedAt 
                                  ? `Started ${new Date(game.startedAt).toLocaleString()}`
                                  : `Created ${new Date(game.createdAt).toLocaleString()}`
                                }
                              </span>
                            </div>
                            
                            <div>
                              <span className="font-medium">Opponent:</span> {opponentName}
                            </div>
                            
                            {game.status === 'Active' && (
                              <div className="flex items-center gap-2">
                                <Trophy className="w-4 h-4" />
                                <span>
                                  You: {game.myScore}/5 • Opponent: {game.opponentScore}/5
                                </span>
                              </div>
                            )}
                          </div>
                        </div>
                        
                        <div className="ml-4">
                          {game.status === 'Pending' || game.status === 'Active' ? (
                            <Button onClick={() => handleJoinGame(game.id)}>
                              <Play className="w-4 h-4 mr-2" />
                              {game.status === 'Pending' ? 'Join' : 'Resume'}
                            </Button>
                          ) : (
                            <Button variant="outline" onClick={() => handleJoinGame(game.id)}>
                              View Results
                            </Button>
                          )}
                        </div>
                      </div>
                    </div>
                  );
                })}
              </div>
            )}
          </CardContent>
        </Card>
      </div>
    </div>
  );
}
