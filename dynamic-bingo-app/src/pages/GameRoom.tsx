import { useEffect, useState } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { Button } from '@/components/ui/button';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Badge } from '@/components/ui/badge';
import { Progress } from '@/components/ui/progress';
import { Dialog, DialogContent, DialogHeader, DialogTitle } from '@/components/ui/dialog';
import { ArrowLeft, Clock, Trophy, RotateCcw } from 'lucide-react';
import { useAuthStore } from '../stores/authStore';
import { useGameStore } from '../stores/gameStore';
import { signalRService } from '../services/signalr';

export function GameRoom() {
  const { gameId } = useParams<{ gameId: string }>();
  const navigate = useNavigate();
  const { user } = useAuthStore();
  const { 
    currentGame, 
    myBoard, 
    opponentBoard, 
    marks, 
    currentTurn, 
    myPlayer, 
    opponentPlayer, 
    isMyTurn, 
    timeRemaining 
  } = useGameStore();
  
  const [selectedCell] = useState<number | null>(null);
  const [showGameEnd, setShowGameEnd] = useState(false);
  const [gameEndResult] = useState<{
    result: 'Win' | 'Lose' | 'Draw';
    reason?: string;
  } | null>(null);
  const [rematchOffered, setRematchOffered] = useState(false);

  useEffect(() => {
    if (!gameId || !user) {
      navigate('/lobby');
      return;
    }

    const initializeGame = async () => {
      try {
        await signalRService.connectToGame();
        await signalRService.joinGame(gameId);
      } catch (error) {
        console.error('Error initializing game:', error);
        navigate('/lobby');
      }
    };

    initializeGame();

    return () => {
      signalRService.disconnect();
    };
  }, [gameId, user, navigate]);

  const handleCellClick = async (number: number) => {
    if (!isMyTurn || !currentGame || !gameId) return;
    
    const isMarked = marks.some(mark => mark.number === number);
    if (isMarked) return;

    try {
      await signalRService.markNumber(gameId, number);
    } catch (error) {
      console.error('Error marking number:', error);
    }
  };

  const handleOfferRematch = async () => {
    if (!gameId) return;
    
    try {
      await signalRService.offerRematch(gameId);
      setRematchOffered(true);
    } catch (error) {
      console.error('Error offering rematch:', error);
    }
  };

  const renderBoard = (board: any, isMyBoard: boolean) => {
    if (!board || !board.layout) return null;

    const boardSize = Math.sqrt(board.layout.length);
    
    return (
      <div 
        className={`grid gap-1 p-4 bg-white rounded-lg shadow-sm border-2 ${
          isMyBoard && isMyTurn ? 'border-blue-500' : 'border-gray-200'
        }`}
        style={{ gridTemplateColumns: `repeat(${boardSize}, 1fr)` }}
      >
        {board.layout.map((number: number, index: number) => {
          const isMarked = marks.some(mark => mark.number === number);
          const canClick = isMyBoard && isMyTurn && !isMarked;
          
          return (
            <button
              key={index}
              onClick={() => canClick && handleCellClick(number)}
              disabled={!canClick}
              className={`
                aspect-square flex items-center justify-center text-sm font-medium rounded
                transition-all duration-200 border-2
                ${isMarked 
                  ? 'bg-blue-500 text-white border-blue-600 shadow-inner' 
                  : canClick 
                    ? 'bg-gray-50 hover:bg-blue-50 border-gray-300 hover:border-blue-300 cursor-pointer'
                    : 'bg-gray-100 border-gray-200 text-gray-500'
                }
                ${selectedCell === number ? 'ring-2 ring-blue-400' : ''}
              `}
            >
              {number}
            </button>
          );
        })}
      </div>
    );
  };

  const renderBingoLetters = (score: number, word: string) => {
    return (
      <div className="flex gap-1 justify-center">
        {word.split('').map((letter, index) => (
          <div
            key={index}
            className={`
              w-8 h-8 flex items-center justify-center font-bold text-lg rounded
              ${index < score 
                ? 'bg-green-500 text-white line-through' 
                : 'bg-gray-200 text-gray-600'
              }
            `}
          >
            {letter}
          </div>
        ))}
      </div>
    );
  };

  if (!currentGame || !myBoard || !user) {
    return (
      <div className="min-h-screen bg-gradient-to-br from-blue-50 to-purple-50 flex items-center justify-center">
        <div className="text-center">
          <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-blue-600 mx-auto mb-4"></div>
          <p className="text-gray-600">Loading game...</p>
        </div>
      </div>
    );
  }

  const opponent = myPlayer?.isCreator ? opponentPlayer : myPlayer?.isCreator === false ? opponentPlayer : null;
  const timeProgress = (timeRemaining / 30) * 100;

  return (
    <div className="min-h-screen bg-gradient-to-br from-blue-50 to-purple-50 p-4">
      <div className="max-w-7xl mx-auto">
        <div className="flex justify-between items-center mb-6">
          <Button variant="outline" onClick={() => navigate('/lobby')}>
            <ArrowLeft className="w-4 h-4 mr-2" />
            Back to Lobby
          </Button>
          
          <div className="text-center">
            <h1 className="text-2xl font-bold text-gray-800">
              {currentGame.word.toUpperCase()} Bingo
            </h1>
            <Badge variant={currentGame.status === 'Active' ? 'default' : 'secondary'}>
              {currentGame.status}
            </Badge>
          </div>
          
          <div className="text-right">
            <p className="text-sm text-gray-600">Game ID</p>
            <p className="font-mono text-sm">{gameId}</p>
          </div>
        </div>

        {/* Turn Timer */}
        {currentGame.status === 'Active' && currentTurn && (
          <Card className="mb-6">
            <CardContent className="p-4">
              <div className="flex items-center justify-between mb-2">
                <div className="flex items-center gap-2">
                  <Clock className="w-4 h-4" />
                  <span className="font-medium">
                    {isMyTurn ? 'Your Turn' : `${opponent?.userId === currentTurn.playerToMoveId ? 'Opponent\'s Turn' : 'Waiting...'}`}
                  </span>
                </div>
                <span className="text-lg font-bold text-blue-600">
                  {timeRemaining}s
                </span>
              </div>
              <Progress value={timeProgress} className="h-2" />
            </CardContent>
          </Card>
        )}

        <div className="grid lg:grid-cols-2 gap-6 mb-6">
          {/* My Board */}
          <Card>
            <CardHeader>
              <CardTitle className="flex items-center justify-between">
                <span>Your Board</span>
                <div className="flex items-center gap-2">
                  <Trophy className="w-4 h-4" />
                  <span>{myPlayer?.score || 0}/5</span>
                </div>
              </CardTitle>
              {currentGame.word && (
                <div className="mt-2">
                  {renderBingoLetters(myPlayer?.score || 0, currentGame.word)}
                </div>
              )}
            </CardHeader>
            <CardContent>
              {renderBoard(myBoard, true)}
            </CardContent>
          </Card>

          {/* Opponent Board */}
          <Card>
            <CardHeader>
              <CardTitle className="flex items-center justify-between">
                <span>Opponent's Board</span>
                <div className="flex items-center gap-2">
                  <Trophy className="w-4 h-4" />
                  <span>{opponentPlayer?.score || 0}/5</span>
                </div>
              </CardTitle>
              {currentGame.word && (
                <div className="mt-2">
                  {renderBingoLetters(opponentPlayer?.score || 0, currentGame.word)}
                </div>
              )}
            </CardHeader>
            <CardContent>
              {renderBoard(opponentBoard, false)}
            </CardContent>
          </Card>
        </div>

        {/* Game End Dialog */}
        <Dialog open={showGameEnd} onOpenChange={setShowGameEnd}>
          <DialogContent>
            <DialogHeader>
              <DialogTitle>
                {gameEndResult?.result === 'Win' && '🎉 You Won!'}
                {gameEndResult?.result === 'Lose' && '😔 You Lost'}
                {gameEndResult?.result === 'Draw' && '🤝 It\'s a Draw!'}
              </DialogTitle>
            </DialogHeader>
            <div className="space-y-4">
              {gameEndResult?.reason && (
                <p className="text-gray-600">{gameEndResult.reason}</p>
              )}
              <div className="flex gap-2">
                <Button 
                  onClick={handleOfferRematch}
                  disabled={rematchOffered}
                  className="flex-1"
                >
                  <RotateCcw className="w-4 h-4 mr-2" />
                  {rematchOffered ? 'Rematch Offered' : 'Offer Rematch'}
                </Button>
                <Button 
                  variant="outline" 
                  onClick={() => navigate('/lobby')}
                  className="flex-1"
                >
                  Return to Lobby
                </Button>
              </div>
            </div>
          </DialogContent>
        </Dialog>
      </div>
    </div>
  );
}
