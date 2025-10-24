import React, { useState, useEffect, useRef } from 'react';
import { getChatSessions, createChatSession, getChatMessages, postChatMessage } from '../services/api';
import './Chat.css';

const Chat = () => {
  const [sessions, setSessions] = useState([]);
  const [currentSession, setCurrentSession] = useState(null);
  const [messages, setMessages] = useState([]);
  const [input, setInput] = useState('');
  const [isLoading, setIsLoading] = useState(false);
  const messagesEndRef = useRef(null);

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
      const formattedMessages = fetchedMessages.flatMap(msg => [
        { sender: 'user', text: msg.message },
        { sender: 'bot', text: msg.response }
      ]);
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
    if (input.trim() === '' || !currentSession) return;

    const userMessage = { sender: 'user', text: input };
    setMessages([...messages, userMessage]);
    setInput('');
    setIsLoading(true);

    try {
      const response = await postChatMessage(currentSession.id, input);
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
      <div className="sidebar">
        <button onClick={handleCreateSession}>New Chat</button>
        <ul>
          {sessions.map(session => (
            <li key={session.id} onClick={() => handleSessionSelect(session)} className={currentSession?.id === session.id ? 'active' : ''}>
              {session.title}
            </li>
          ))}
        </ul>
      </div>
      <div className="chat-window">
        <div className="chat-header">
          <h1>{currentSession ? currentSession.title : 'EduShelf Assistant'}</h1>
        </div>
        <div className="chat-messages">
          {messages.map((msg, index) => (
            <div key={index} className={`message ${msg.sender}`}>
              {msg.text}
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
            type="text"
            className="chat-input"
            value={input}
            onChange={(e) => setInput(e.target.value)}
            placeholder="Type your message..."
            disabled={isLoading || !currentSession}
          />
          <button type="submit" className="send-button" disabled={isLoading || !currentSession}>
            Send
          </button>
        </form>
      </div>
    </div>
  );
};

export default Chat;