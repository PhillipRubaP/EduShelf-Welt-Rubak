import API_BASE_URL from '../config';

const getAuthHeaders = () => {
  const token = localStorage.getItem('token');
  return token ? { 'Authorization': `Bearer ${token}` } : {};
};

const api = {
  get: async (endpoint, options = {}) => {
    const response = await fetch(`${API_BASE_URL}${endpoint}`, {
      headers: getAuthHeaders(),
      ...options,
    });
    if (options.responseType === 'blob') {
      return response.blob();
    }
    return response.json();
  },

  post: async (endpoint, body) => {
    const response = await fetch(`${API_BASE_URL}${endpoint}`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
        ...getAuthHeaders(),
      },
      body: JSON.stringify(body),
    });
    return response.json();
  },
  
  postForm: async (endpoint, formData) => {
    const response = await fetch(`${API_BASE_URL}${endpoint}`, {
        method: 'POST',
        headers: {
            ...getAuthHeaders(),
        },
        body: formData,
    });
    return response.json();
  },

  put: async (endpoint, body) => {
    const response = await fetch(`${API_BASE_URL}${endpoint}`, {
      method: 'PUT',
      headers: {
        'Content-Type': 'application/json',
        ...getAuthHeaders(),
      },
      body: JSON.stringify(body),
    });
    return response.json();
  },

  delete: async (endpoint) => {
    const response = await fetch(`${API_BASE_URL}${endpoint}`, {
      method: 'DELETE',
      headers: getAuthHeaders(),
    });
    return response;
  },

  download: async (endpoint) => {
    return api.get(endpoint, { responseType: 'blob' });
  },
};

export const getQuizzes = () => api.get('/quizzes');
export const createQuiz = (quizData) => api.post('/quizzes', quizData);
export const deleteQuiz = (quizId) => api.delete(`/quizzes/${quizId}`);

export const getFlashcards = () => api.get('/flashcards');
const decodeToken = (token) => {
    try {
        return JSON.parse(atob(token.split('.')[1]));
    } catch (e) {
        return null;
    }
};

export const createFlashcard = (flashcardData) => {
    const token = localStorage.getItem('token');
    const decodedToken = decodeToken(token);
    console.log(decodedToken); // This will show the token structure in the console
    const userId = decodedToken ? (decodedToken['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier'] || decodedToken.sub) : null;

    if (!userId) {
        return Promise.reject('User not found');
    }

    const dataToSend = {
        UserId: parseInt(userId, 10),
        Question: flashcardData.front,
        Answer: flashcardData.back,
        Tags: flashcardData.tags || [],
    };

    return api.post('/flashcards', dataToSend);
};
export const deleteFlashcard = (flashcardId) => api.delete(`/flashcards/${flashcardId}`);

export default api;