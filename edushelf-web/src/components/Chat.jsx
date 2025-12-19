import React, { useState, useEffect, useRef } from 'react';
import ReactMarkdown from 'react-markdown';
import rehypeHighlight from 'rehype-highlight';
import { getChatSessions, createChatSession, getChatMessages, postChatMessage, deleteChatSession } from '../services/api';
import './Chat.css';

const Chat = () => {
  const [sessions, setSessions] = useState([]);
  const [currentSession, setCurrentSession] = useState(null);
  const [messages, setMessages] = useState([]);
  const [input, setInput] = useState('');
  const [image, setImage] = useState(null);
  const [isLoading, setIsLoading] = useState(false);
  const [sidebarWidth, setSidebarWidth] = useState(250);
  const messagesEndRef = useRef(null);
  const fileInputRef = useRef(null);
  const sidebarRef = useRef(null);

  const handleMouseDown = (e) => {
    e.preventDefault();
    document.addEventListener('mousemove', handleMouseMove);
    document.addEventListener('mouseup', handleMouseUp);
  };

  const handleMouseMove = (e) => {
    if (sidebarRef.current) {
      const newWidth = e.clientX - sidebarRef.current.getBoundingClientRect().left;
      if (newWidth > 200 && newWidth < 500) {
        setSidebarWidth(newWidth);
      }
    }
  };

  const handleMouseUp = () => {
    document.removeEventListener('mousemove', handleMouseMove);
    document.removeEventListener('mouseup', handleMouseUp);
  };

  useEffect(() => {
    fetchSessions();
  }, []);

  useEffect(() => {
    if (currentSession) {
      fetchMessages(currentSession.id);
    } else if (sessions.length > 0) {
      setCurrentSession(sessions[0]);
    } else {
      setMessages([]);
    }
  }, [currentSession, sessions]);

  const fetchSessions = async () => {
    try {
      const fetchedSessions = await getChatSessions();
      setSessions(fetchedSessions);
      if (fetchedSessions.length > 0) {
        setCurrentSession(fetchedSessions[0]);
      }
    } catch (error) {
      console.error('Error fetching sessions:', error);
    }
  };

  const fetchMessages = async (sessionId) => {
    try {
      const fetchedMessages = await getChatMessages(sessionId);
      const formattedMessages = fetchedMessages.flatMap(msg => {
        const userMessage = { 
          sender: 'user', 
          text: msg.message, 
          image: msg.imagePath 
        };
        if (msg.response) {
          const botMessage = { 
            sender: 'bot', 
            text: msg.response 
          };
          return [userMessage, botMessage];
        }
        return [userMessage];
      });
      setMessages(formattedMessages);
    } catch (error) {
      console.error('Error fetching messages:', error);
    }
  };

  const handleSessionSelect = (session) => {
    setCurrentSession(session);
  };

  const handleCreateSession = async () => {
    const title = prompt("Enter a title for the new session:");
    if (title) {
      try {
        const newSession = await createChatSession(title);
        setSessions([...sessions, newSession]);
        setCurrentSession(newSession);
      } catch (error) {
        console.error('Error creating session:', error);
      }
    }
  };

  const handleDeleteSession = async (sessionId, e) => {
    e.stopPropagation();
    if (window.confirm('Are you sure you want to delete this session?')) {
      try {
        await deleteChatSession(sessionId);
        const updatedSessions = sessions.filter(session => session.id !== sessionId);
        setSessions(updatedSessions);
        if (currentSession && currentSession.id === sessionId) {
          setCurrentSession(updatedSessions.length > 0 ? updatedSessions[0] : null);
        }
      } catch (error) {
        console.error('Error deleting session:', error);
      }
    }
  };

  const handleSend = async (e) => {
    e.preventDefault();
    if ((input.trim() === '' && !image) || !currentSession) return;

    const userMessage = { sender: 'user', text: input, image: image ? URL.createObjectURL(image) : null };
    setMessages([...messages, userMessage]);
    setInput('');
    setImage(null);
    setIsLoading(true);

    try {
      const response = await postChatMessage(currentSession.id, input, image);
      const botMessage = { sender: 'bot', text: response.response };
      setMessages(prevMessages => [...prevMessages, botMessage]);
    } catch (error) {
      console.error('Error sending message:', error);
      const errorMessage = { sender: 'bot', text: 'Sorry, something went wrong.' };
      setMessages(prevMessages => [...prevMessages, errorMessage]);
    } finally {
      setIsLoading(false);
    }
  };

  const scrollToBottom = () => {
    messagesEndRef.current?.scrollIntoView({ behavior: "smooth" });
  };

  useEffect(() => {
    scrollToBottom();
  }, [messages]);

  return (
    <div className="chat-container">
      <div className="sidebar" ref={sidebarRef} style={{ width: sidebarWidth }}>
        <button onClick={handleCreateSession}>New Chat</button>
        <ul>
          {sessions.map(session => (
            <li key={session.id} onClick={() => handleSessionSelect(session)} className={currentSession?.id === session.id ? 'active' : ''}>
              <span className="session-title">{session.title}</span>
              <span className="delete-session" onClick={(e) => handleDeleteSession(session.id, e)}>
                <svg xmlns="http://www.w3.org/2000/svg" width="16" height="16" fill="currentColor" className="bi bi-trash" viewBox="0 0 16 16">
                  <path d="M5.5 5.5A.5.5 0 0 1 6 6v6a.5.5 0 0 1-1 0V6a.5.5 0 0 1 .5-.5zm2.5 0a.5.5 0 0 1 .5.5v6a.5.5 0 0 1-1 0V6a.5.5 0 0 1 .5-.5zm3 .5a.5.5 0 0 0-1 0v6a.5.5 0 0 0 1 0V6z"/>
                  <path fillRule="evenodd" d="M14.5 3a1 1 0 0 1-1 1H13v9a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2V4h-.5a1 1 0 0 1-1-1V2a1 1 0 0 1 1-1H6a1 1 0 0 1 1-1h2a1 1 0 0 1 1 1h3.5a1 1 0 0 1 1 1v1zM4.118 4 4 4.059V13a1 1 0 0 0 1 1h6a1 1 0 0 0 1-1V4.059L11.882 4H4.118zM2.5 3V2h11v1h-11z"/>
                </svg>
              </span>
            </li>
          ))}
        </ul>
      </div>
      <div className="resizer" onMouseDown={handleMouseDown} />
      <div className="chat-window">
        <div className="chat-header">
          <h1>{currentSession ? currentSession.title : 'EduShelf Assistant'}</h1>
        </div>
        <div className="chat-messages">
          {messages.map((msg, index) => (
            <div key={index} className={`message ${msg.sender}`}>
              <ReactMarkdown rehypePlugins={[rehypeHighlight]}>{msg.text}</ReactMarkdown>
              {msg.image && <img src={msg.image} alt="user upload" className="chat-image" />}
            </div>
          ))}
          {isLoading && (
            <div className="message bot">
              ...
            </div>
          )}
          <div ref={messagesEndRef} />
        </div>
        <form className="chat-input-form" onSubmit={handleSend}>
          <input
            type="file"
            id="file-input"
            ref={fileInputRef}
            className="file-input"
            onChange={(e) => setImage(e.target.files[0])}
            accept="image/*"
          />
          <button type="button" className="upload-button" onClick={() => fileInputRef.current.click()}>
            Upload Image
          </button>
          <input
            type="text"
            className="chat-input"
            value={input}
            onChange={(e) => setInput(e.target.value)}
            placeholder="Type your message..."
            disabled={isLoading || !currentSession}
          />
          {image && (
            <div className="image-preview">
              <img src={URL.createObjectURL(image)} alt="preview" />
              <button onClick={() => setImage(null)}>x</button>
            </div>
          )}
          <button type="submit" className="send-button" disabled={isLoading || !currentSession}>
            Send
          </button>
        </form>
      </div>
    </div>
  );
};

export default Chat;