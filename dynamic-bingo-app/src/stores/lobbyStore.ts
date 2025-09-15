import { create } from 'zustand';
import { LobbyState, User, OpenChallenge, Friendship, Presence } from '../types';

interface LobbyStore extends LobbyState {
  setOnlineUsers: (users: User[]) => void;
  setOpenChallenges: (challenges: OpenChallenge[]) => void;
  addChallenge: (challenge: OpenChallenge) => void;
  removeChallenge: (challengeId: string) => void;
  setFriends: (friends: Friendship[]) => void;
  addFriend: (friendship: Friendship) => void;
  updateFriendship: (friendship: Friendship) => void;
  setPresence: (userId: string, presence: Presence) => void;
  updateUserPresence: (userId: string, status: Presence['status']) => void;
}

export const useLobbyStore = create<LobbyStore>((set) => ({
  onlineUsers: [],
  openChallenges: [],
  friends: [],
  presence: {},

  setOnlineUsers: (onlineUsers) => set({ onlineUsers }),
  
  setOpenChallenges: (openChallenges) => set({ openChallenges }),
  
  addChallenge: (challenge) => set((state) => ({
    openChallenges: [...state.openChallenges, challenge]
  })),
  
  removeChallenge: (challengeId) => set((state) => ({
    openChallenges: state.openChallenges.filter(c => c.id !== challengeId)
  })),
  
  setFriends: (friends) => set({ friends }),
  
  addFriend: (friendship) => set((state) => ({
    friends: [...state.friends, friendship]
  })),
  
  updateFriendship: (friendship) => set((state) => ({
    friends: state.friends.map(f => 
      f.id === friendship.id ? friendship : f
    )
  })),
  
  setPresence: (userId, presence) => set((state) => ({
    presence: { ...state.presence, [userId]: presence }
  })),
  
  updateUserPresence: (userId, status) => set((state) => ({
    presence: {
      ...state.presence,
      [userId]: {
        ...state.presence[userId],
        status,
        lastSeenAt: new Date().toISOString()
      }
    }
  })),
}));
