import { HubConnection, HubConnectionBuilder, LogLevel } from '@microsoft/signalr';
import { useAuthStore } from '../stores/authStore';
import { useGameStore } from '../stores/gameStore';
import { useLobbyStore } from '../stores/lobbyStore';

class SignalRService {
  private lobbyConnection: HubConnection | null = null;
  private gameConnection: HubConnection | null = null;

  async connectToLobby(): Promise<void> {
    if (this.lobbyConnection?.state === 'Connected') {
      return;
    }

    const API_BASE_URL = import.meta.env.VITE_API_URL || 'http://localhost:5000';
    
    this.lobbyConnection = new HubConnectionBuilder()
      .withUrl(`${API_BASE_URL}/hubs/lobby`)
      .withAutomaticReconnect()
      .configureLogging(LogLevel.Information)
      .build();

    this.setupLobbyHandlers();
    
    try {
      await this.lobbyConnection.start();
      console.log('Connected to lobby hub');
    } catch (error) {
      console.error('Error connecting to lobby hub:', error);
      throw error;
    }
  }

  async connectToGame(): Promise<void> {
    if (this.gameConnection?.state === 'Connected') {
      return;
    }

    const API_BASE_URL = import.meta.env.VITE_API_URL || 'http://localhost:5000';
    
    this.gameConnection = new HubConnectionBuilder()
      .withUrl(`${API_BASE_URL}/hubs/game`)
      .withAutomaticReconnect()
      .configureLogging(LogLevel.Information)
      .build();

    this.setupGameHandlers();
    
    try {
      await this.gameConnection.start();
      console.log('Connected to game hub');
    } catch (error) {
      console.error('Error connecting to game hub:', error);
      throw error;
    }
  }

  private setupLobbyHandlers(): void {
    if (!this.lobbyConnection) return;

    const lobbyStore = useLobbyStore.getState();

    this.lobbyConnection.on('ChallengeCreated', (challenge) => {
      lobbyStore.addChallenge(challenge);
    });

    this.lobbyConnection.on('ChallengeCancelled', (challengeId) => {
      lobbyStore.removeChallenge(challengeId);
    });

    this.lobbyConnection.on('ChallengeAccepted', ({ challengeId, gameId }) => {
      lobbyStore.removeChallenge(challengeId);
      window.location.href = `/game/${gameId}`;
    });

    this.lobbyConnection.on('LobbyUsersUpdated', (users) => {
      lobbyStore.setOnlineUsers(users);
    });

    this.lobbyConnection.on('PresenceUpdated', ({ userId, status }) => {
      lobbyStore.updateUserPresence(userId, status);
    });

    this.lobbyConnection.on('FriendRequestReceived', (request) => {
      lobbyStore.addFriend(request);
    });

    this.lobbyConnection.on('FriendRequestUpdated', (request) => {
      lobbyStore.updateFriendship(request);
    });
  }

  private setupGameHandlers(): void {
    if (!this.gameConnection) return;

    const gameStore = useGameStore.getState();

    this.gameConnection.on('GameState', (gameState) => {
      gameStore.setCurrentGame(gameState.game);
      gameStore.setMyBoard(gameState.myBoard);
      gameStore.setOpponentBoard(gameState.opponentBoard);
      gameStore.setMarks(gameState.marks);
      gameStore.setPlayers(gameState.myPlayer, gameState.opponentPlayer);
    });

    this.gameConnection.on('TurnStarted', ({ playerId, turnIndex, expiresAt }) => {
      const authStore = useAuthStore.getState();
      const isMyTurn = playerId === authStore.user?.id;
      gameStore.updateMyTurn(isMyTurn);
      gameStore.setCurrentTurn({
        id: '',
        gameId: '',
        index: turnIndex,
        playerToMoveId: playerId,
        startedAt: new Date().toISOString(),
        expiresAt,
      });
    });

    this.gameConnection.on('NumberMarked', ({ byUserId, number, turnIndex }) => {
      gameStore.addMark({
        id: '',
        gameId: '',
        number,
        markedByUserId: byUserId,
        markedAt: new Date().toISOString(),
        turnIndex,
      });
    });

    this.gameConnection.on('ScoreUpdated', ({ userId, newScore }) => {
      const { myPlayer, opponentPlayer } = useGameStore.getState();
      const authStore = useAuthStore.getState();
      
      if (userId === authStore.user?.id && myPlayer) {
        gameStore.setPlayers({ ...myPlayer, score: newScore }, opponentPlayer);
      } else if (opponentPlayer) {
        gameStore.setPlayers(myPlayer, { ...opponentPlayer, score: newScore });
      }
    });

    this.gameConnection.on('GameEnded', ({ result, winnerId, reason }) => {
      console.log('Game ended:', { result, winnerId, reason });
    });

    this.gameConnection.on('RematchReady', ({ newGameId }) => {
      window.location.href = `/game/${newGameId}`;
    });

    this.gameConnection.on('PenaltyApplied', ({ userId, type, details }) => {
      console.log('Penalty applied:', { userId, type, details });
    });
  }

  async createOpenChallenge(challenge: {
    visibility: string;
    word: string;
    fillMode: string;
    starterChoice: string;
  }): Promise<void> {
    if (!this.lobbyConnection) throw new Error('Not connected to lobby');
    await this.lobbyConnection.invoke('CreateOpenChallenge', challenge);
  }

  async cancelOpenChallenge(challengeId: string): Promise<void> {
    if (!this.lobbyConnection) throw new Error('Not connected to lobby');
    await this.lobbyConnection.invoke('CancelOpenChallenge', challengeId);
  }

  async acceptChallenge(challengeId: string): Promise<{ gameId: string }> {
    if (!this.lobbyConnection) throw new Error('Not connected to lobby');
    return await this.lobbyConnection.invoke('AcceptChallenge', challengeId);
  }

  async sendFriendRequest(targetUserId: string): Promise<void> {
    if (!this.lobbyConnection) throw new Error('Not connected to lobby');
    await this.lobbyConnection.invoke('SendFriendRequest', targetUserId);
  }

  async respondFriendRequest(requestId: string, accept: boolean): Promise<void> {
    if (!this.lobbyConnection) throw new Error('Not connected to lobby');
    await this.lobbyConnection.invoke('RespondFriendRequest', requestId, accept);
  }

  async joinGame(gameId: string): Promise<void> {
    if (!this.gameConnection) throw new Error('Not connected to game');
    await this.gameConnection.invoke('JoinGame', gameId);
  }

  async submitManualBoard(gameId: string, layout: number[]): Promise<void> {
    if (!this.gameConnection) throw new Error('Not connected to game');
    await this.gameConnection.invoke('SubmitManualBoard', gameId, layout);
  }

  async markNumber(gameId: string, number: number): Promise<void> {
    if (!this.gameConnection) throw new Error('Not connected to game');
    await this.gameConnection.invoke('MarkNumber', gameId, number);
  }

  async offerRematch(gameId: string): Promise<void> {
    if (!this.gameConnection) throw new Error('Not connected to game');
    await this.gameConnection.invoke('OfferRematch', gameId);
  }

  async acceptRematch(gameId: string): Promise<void> {
    if (!this.gameConnection) throw new Error('Not connected to game');
    await this.gameConnection.invoke('AcceptRematch', gameId);
  }

  async disconnect(): Promise<void> {
    if (this.lobbyConnection) {
      await this.lobbyConnection.stop();
      this.lobbyConnection = null;
    }
    if (this.gameConnection) {
      await this.gameConnection.stop();
      this.gameConnection = null;
    }
  }
}

export const signalRService = new SignalRService();
