export interface User {
  id: string;
  email?: string;
  displayName: string;
  isGuest: boolean;
  createdAt: string;
  lastActiveAt: string;
}

export interface GameSettings {
  word: string;
  fillMode: 'Sequential' | 'Random' | 'Manual';
  starterChoice: 'Creator' | 'Opponent' | 'Random';
}

export interface Game {
  id: string;
  word: string;
  n: number;
  creatorId: string;
  opponentId?: string;
  settings: GameSettings;
  status: 'Pending' | 'Active' | 'Finished' | 'Forfeited' | 'Draw';
  starter: 'Creator' | 'Opponent';
  createdAt: string;
  startedAt?: string;
  finishedAt?: string;
}

export interface GamePlayer {
  id: string;
  gameId: string;
  userId: string;
  isCreator: boolean;
  idleCount: number;
  score: number;
  bingoLettersCrossed: number;
  isWinner?: boolean;
  forfeitReason?: string;
}

export interface Board {
  id: string;
  gameId: string;
  userId: string;
  layout: number[];
  fillMode: 'Sequential' | 'Random' | 'Manual';
}

export interface Mark {
  id: string;
  gameId: string;
  number: number;
  markedByUserId: string;
  markedAt: string;
  turnIndex: number;
}

export interface Turn {
  id: string;
  gameId: string;
  index: number;
  playerToMoveId: string;
  startedAt: string;
  expiresAt: string;
  resolvedAt?: string;
  outcome?: 'Mark' | 'AutoMark' | 'Forfeit';
  markedNumber?: number;
}

export interface OpenChallenge {
  id: string;
  creatorId: string;
  visibility: 'Public' | 'Friends' | 'Private';
  word: string;
  fillMode: 'Sequential' | 'Random' | 'Manual';
  starterChoice: 'Creator' | 'Opponent' | 'Random';
  createdAt: string;
  isActive: boolean;
  creator: User;
}

export interface Friendship {
  id: string;
  userAId: string;
  userBId: string;
  status: 'Pending' | 'Accepted' | 'Blocked';
  createdAt: string;
  acceptedAt?: string;
  userA: User;
  userB: User;
}

export interface Presence {
  userId: string;
  status: 'Available' | 'InGame' | 'Busy';
  lastSeenAt: string;
}

export interface Ban {
  id: string;
  userId: string;
  reason: string;
  durationSeconds: number;
  startsAt: string;
  endsAt: string;
}

export interface AuthState {
  user: User | null;
  isAuthenticated: boolean;
  isLoading: boolean;
}

export interface GameState {
  currentGame: Game | null;
  myBoard: Board | null;
  opponentBoard: Board | null;
  marks: Mark[];
  currentTurn: Turn | null;
  myPlayer: GamePlayer | null;
  opponentPlayer: GamePlayer | null;
  isMyTurn: boolean;
  timeRemaining: number;
}

export interface LobbyState {
  onlineUsers: User[];
  openChallenges: OpenChallenge[];
  friends: Friendship[];
  presence: Record<string, Presence>;
}
