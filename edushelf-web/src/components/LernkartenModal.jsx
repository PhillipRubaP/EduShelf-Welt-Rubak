import React, { useState } from 'react';
import './LernkartenModal.css';

const LernkartenModal = ({ addCard, closeModal }) => {
  const [front, setFront] = useState('');
  const [back, setBack] = useState('');

  const handleSubmit = (e) => {
    e.preventDefault();
    if (front.trim() !== '' && back.trim() !== '') {
      addCard({ front, back });
      closeModal();
    }
  };

  return (
    <div className="modal-overlay">
      <div className="modal-content">
        <h2 className="text-2xl font-bold mb-4">Neue Lernkarte erstellen</h2>
        <form onSubmit={handleSubmit}>
          <div className="flex flex-col mb-2">
            <label htmlFor="front" className="mb-1 text-gray-700">Vorderseite:</label>
            <input
              type="text"
              id="front"
              value={front}
              onChange={(e) => setFront(e.target.value)}
              className="p-2 border rounded lernkarten-input"
              placeholder="Frage"
            />
          </div>
          <div className="flex flex-col mb-2">
            <label htmlFor="back" className="mb-1 text-gray-700">Rückseite:</label>
            <input
              type="text"
              id="back"
              value={back}
              onChange={(e) => setBack(e.target.value)}
              className="p-2 border rounded lernkarten-input"
              placeholder="Antwort"
            />
          </div>
          <div className="flex justify-end gap-2 mt-4">
            <button type="button" onClick={closeModal} className="bg-green-500 text-white p-2 rounded">
              Abbrechen
            </button>
            <button type="submit" className="bg-green-500 text-white p-2 rounded">
              Karte hinzufügen
            </button>
          </div>
        </form>
      </div>
    </div>
  );
};

export default LernkartenModal;