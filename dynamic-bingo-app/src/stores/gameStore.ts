import { create } from 'zustand';
import { GameState, Game, Board, Mark, Turn, GamePlayer } from '../types';

interface GameStore extends GameState {
  setCurrentGame: (game: Game | null) => void;
  setMyBoard: (board: Board | null) => void;
  setOpponentBoard: (board: Board | null) => void;
  setMarks: (marks: Mark[]) => void;
  addMark: (mark: Mark) => void;
  setCurrentTurn: (turn: Turn | null) => void;
  setPlayers: (myPlayer: GamePlayer | null, opponentPlayer: GamePlayer | null) => void;
  setTimeRemaining: (time: number) => void;
  updateMyTurn: (isMyTurn: boolean) => void;
  resetGame: () => void;
}

export const useGameStore = create<GameStore>((set) => ({
  currentGame: null,
  myBoard: null,
  opponentBoard: null,
  marks: [],
  currentTurn: null,
  myPlayer: null,
  opponentPlayer: null,
  isMyTurn: false,
  timeRemaining: 30,

  setCurrentGame: (currentGame) => set({ currentGame }),
  
  setMyBoard: (myBoard) => set({ myBoard }),
  
  setOpponentBoard: (opponentBoard) => set({ opponentBoard }),
  
  setMarks: (marks) => set({ marks }),
  
  addMark: (mark) => set((state) => ({ 
    marks: [...state.marks, mark] 
  })),
  
  setCurrentTurn: (currentTurn) => set({ currentTurn }),
  
  setPlayers: (myPlayer, opponentPlayer) => set({ 
    myPlayer, 
    opponentPlayer 
  }),
  
  setTimeRemaining: (timeRemaining) => set({ timeRemaining }),
  
  updateMyTurn: (isMyTurn) => set({ isMyTurn }),
  
  resetGame: () => set({
    currentGame: null,
    myBoard: null,
    opponentBoard: null,
    marks: [],
    currentTurn: null,
    myPlayer: null,
    opponentPlayer: null,
    isMyTurn: false,
    timeRemaining: 30,
  }),
}));
