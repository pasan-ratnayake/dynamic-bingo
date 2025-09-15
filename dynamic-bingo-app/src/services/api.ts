const API_BASE_URL = import.meta.env.VITE_API_URL || 'http://localhost:5000/api';

class ApiService {
  private async request<T>(
    endpoint: string,
    options: RequestInit = {}
  ): Promise<T> {
    const url = `${API_BASE_URL}${endpoint}`;
    console.log('API Request URL:', url, 'Base:', API_BASE_URL, 'Endpoint:', endpoint);
    
    const config: RequestInit = {
      headers: {
        'Content-Type': 'application/json',
        ...options.headers,
      },
      ...options,
    };

    const token = localStorage.getItem('auth-token');
    if (token) {
      config.headers = {
        ...config.headers,
        Authorization: `Bearer ${token}`,
      };
    }

    const response = await fetch(url, config);
    
    if (!response.ok) {
      throw new Error(`API Error: ${response.status} ${response.statusText}`);
    }

    return response.json();
  }

  async requestMagicLink(email: string): Promise<void> {
    await this.request('/auth/magic-links', {
      method: 'POST',
      body: JSON.stringify({ email }),
    });
  }

  async consumeMagicLink(token: string): Promise<{ accessToken: string }> {
    return this.request('/auth/magic-links/consume', {
      method: 'POST',
      body: JSON.stringify({ token }),
    });
  }

  async getMe(): Promise<any> {
    return this.request('/me');
  }

  async updateProfile(data: { displayName?: string; email?: string }): Promise<any> {
    return this.request('/me', {
      method: 'PATCH',
      body: JSON.stringify(data),
    });
  }

  async deleteAccount(): Promise<void> {
    await this.request('/me', {
      method: 'DELETE',
    });
  }

  async convertGuest(email: string): Promise<void> {
    await this.request('/guests/convert', {
      method: 'POST',
      body: JSON.stringify({ email }),
    });
  }

  async getFriends(userId?: string): Promise<any[]> {
    const userIdParam = userId || localStorage.getItem('user-id') || '';
    return this.request(`/friends?userId=${userIdParam}`);
  }

  async sendFriendRequest(targetUserId: string): Promise<any> {
    return this.request('/friends/requests', {
      method: 'POST',
      body: JSON.stringify({ targetUserId }),
    });
  }

  async respondToFriendRequest(requestId: string, accept: boolean): Promise<any> {
    return this.request(`/friends/requests/${requestId}/respond`, {
      method: 'POST',
      body: JSON.stringify({ accept }),
    });
  }

  async getLobbyUsers(): Promise<any[]> {
    return this.request('/lobby/users');
  }

  async createChallenge(challenge: {
    visibility: string;
    word: string;
    fillMode: string;
    starterChoice: string;
  }): Promise<any> {
    return this.request('/challenges', {
      method: 'POST',
      body: JSON.stringify(challenge),
    });
  }

  async cancelChallenge(challengeId: string): Promise<void> {
    await this.request(`/challenges/${challengeId}/cancel`, {
      method: 'POST',
    });
  }

  async acceptChallenge(challengeId: string): Promise<{ gameId: string }> {
    return this.request(`/challenges/${challengeId}/accept`, {
      method: 'POST',
    });
  }

  async getOngoingGames(userId?: string): Promise<any[]> {
    const userIdParam = userId || localStorage.getItem('user-id') || '';
    return this.request(`/games/ongoing?userId=${userIdParam}`);
  }

  async getGame(gameId: string): Promise<any> {
    return this.request(`/games/${gameId}`);
  }
}

export const apiService = new ApiService();
