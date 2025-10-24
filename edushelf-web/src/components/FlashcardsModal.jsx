import React, { useState } from 'react';
import './FlashcardsModal.css';

const FlashcardsModal = ({ addCard, closeModal }) => {
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
        <h2>Create New Flashcard</h2>
        <form onSubmit={handleSubmit}>
          <div className="form-group">
            <label htmlFor="front">Front:</label>
            <input
              type="text"
              id="front"
              value={front}
              onChange={(e) => setFront(e.target.value)}
              placeholder="Question"
            />
          </div>
          <div className="form-group">
            <label htmlFor="back">Back:</label>
            <textarea
              id="back"
              value={back}
              onChange={(e) => setBack(e.target.value)}
              placeholder="Answer"
            />
          </div>
          <div className="form-group">
            <label htmlFor="tags">Tags:</label>
            <input
              type="text"
              id="tags"
              value={tags}
              onChange={(e) => setTags(e.target.value)}
              placeholder="e.g. Math, Formulas"
            />
          </div>
          <div className="modal-actions">
            <button type="submit">
              Add Card
            </button>
            <button type="button" onClick={closeModal}>
              Cancel
            </button>
          </div>
        </form>
      </div>
    </div>
  );
};

export default FlashcardsModal;