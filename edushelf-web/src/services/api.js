import API_BASE_URL from '../config';

const api = {
  get: async (endpoint, options = {}) => {
    const response = await fetch(`${API_BASE_URL}${endpoint}`, {
      credentials: 'include',
      ...options,
    });
    if (!response.ok) {
      const text = await response.text();
      console.error(`API Error (${response.status}):`, text);
      throw new Error(`API Error: ${response.status} ${response.statusText}`);
    }
    if (options.responseType === 'blob') {
      return response.blob();
    }
    try {
      return await response.json();
    } catch (e) {
      console.error('Failed to parse JSON response:', e);
      throw e;
    }
  },

  post: async (endpoint, body) => {
    const response = await fetch(`${API_BASE_URL}${endpoint}`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
      },
      credentials: 'include',
      body: JSON.stringify(body),
    });
    if (!response.ok) {
      const text = await response.text();
      console.error(`API Error (${response.status}):`, text);
      throw new Error(`API Error: ${response.status} ${response.statusText}`);
    }
    return response.json();
  },

  postForm: async (endpoint, formData) => {
    const response = await fetch(`${API_BASE_URL}${endpoint}`, {
      method: 'POST',
      credentials: 'include',
      body: formData,
    });
    return response.json();
  },

  put: async (endpoint, body) => {
    const response = await fetch(`${API_BASE_URL}${endpoint}`, {
      method: 'PUT',
      headers: {
        'Content-Type': 'application/json',
      },
      credentials: 'include',
      body: JSON.stringify(body),
    });
    const text = await response.text();
    return text ? JSON.parse(text) : {};
  },

  delete: async (endpoint) => {
    const response = await fetch(`${API_BASE_URL}${endpoint}`, {
      method: 'DELETE',
      credentials: 'include',
    });
    return response;
  },

  download: async (endpoint) => {
    return api.get(endpoint, { responseType: 'blob' });
  },
};
export const getChatSessions = () => api.get('/Chat/sessions');
export const createChatSession = (title) => api.post('/Chat/sessions', { title });
export const getChatMessages = (sessionId) => api.get(`/Chat/sessions/${sessionId}/messages`);
export const postChatMessage = (sessionId, message, image) => {
  const formData = new FormData();
  formData.append('chatSessionId', sessionId);
  formData.append('message', message);
  if (image) {
    formData.append('image', image);
  }
  return api.postForm('/Chat/message', formData);
};

export const getQuizzes = () => api.get('/quizzes');
export const createQuiz = (quizData) => api.post('/quizzes', quizData);
export const deleteQuiz = (quizId) => api.delete(`/quizzes/${quizId}`);
export const updateQuiz = (quizId, quizData) => api.put(`/quizzes/${quizId}`, quizData);

export const getFlashcards = () => api.get('/flashcards');

export const createFlashcard = (flashcardData) => {
  // The backend will associate the user from the session
  return api.post('/flashcards', flashcardData);
};
export const deleteFlashcard = (flashcardId) => api.delete(`/flashcards/${flashcardId}`);

export const updateFlashcard = (card) => {
  const dataToSend = {
    Question: card.question,
    Answer: card.answer,
    Tags: card.tags || [],
  };
  return api.put(`/flashcards/${card.id}`, dataToSend);
};

export const getDocuments = () => api.get('/documents');
export const getTags = () => api.get('/tags');
export const getFlashcardsByTag = (tagId) => api.get(`/flashcards/tag/${tagId}`);

export default api;