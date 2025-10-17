import React, { useState } from 'react';
import './LernkartenModal.css';

const LernkartenModal = ({ addCard, closeModal }) => {
  const [front, setFront] = useState('');
  const [back, setBack] = useState('');
  const [tags, setTags] = useState('');

  const handleSubmit = (e) => {
    e.preventDefault();
    if (front.trim() !== '' && back.trim() !== '') {
      const tagsArray = tags.split(',').map(tag => tag.trim()).filter(tag => tag);
      addCard({ front, back, tags: tagsArray });
      closeModal();
    }
  };

  return (
    <div className="modal-overlay">
      <div className="modal-content">
        <h2>Neue Lernkarte erstellen</h2>
        <form onSubmit={handleSubmit}>
          <div className="form-group">
            <label htmlFor="front">Vorderseite:</label>
            <input
              type="text"
              id="front"
              value={front}
              onChange={(e) => setFront(e.target.value)}
              placeholder="Frage"
            />
          </div>
          <div className="form-group">
            <label htmlFor="back">Rückseite:</label>
            <textarea
              id="back"
              value={back}
              onChange={(e) => setBack(e.target.value)}
              placeholder="Antwort"
            />
          </div>
          <div className="form-group">
            <label htmlFor="tags">Tags:</label>
            <input
              type="text"
              id="tags"
              value={tags}
              onChange={(e) => setTags(e.target.value)}
              placeholder="z.B. Mathe, Formeln"
            />
          </div>
          <div className="modal-actions">
            <button type="button" onClick={closeModal}>
              Abbrechen
            </button>
            <button type="submit">
              Karte hinzufügen
            </button>
          </div>
        </form>
      </div>
    </div>
  );
};

export default LernkartenModal;