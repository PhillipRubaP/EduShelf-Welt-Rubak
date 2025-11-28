import React, { useState, useEffect, useRef } from 'react';
import { getChatSessions, createChatSession, getChatMessages, postChatMessage } from '../services/api';
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
    }
  }, [currentSession]);

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
              {session.title}
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
              {msg.text}
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