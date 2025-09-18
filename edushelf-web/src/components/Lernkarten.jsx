import React, { useState } from 'react';
import LernkartenModal from './LernkartenModal';

const Lernkarten = () => {
  const [cards, setCards] = useState([]);
  const [flippedCard, setFlippedCard] = useState(null);
  const [isModalOpen, setIsModalOpen] = useState(false);

  const addCard = (card) => {
    setCards([...cards, card]);
  };

  const deleteCard = (index) => {
    const newCards = [...cards];
    newCards.splice(index, 1);
    setCards(newCards);
  };

  const flipCard = (index) => {
    setFlippedCard(flippedCard === index ? null : index);
  };

  const openModal = () => {
    setIsModalOpen(true);
  };

  const closeModal = () => {
    setIsModalOpen(false);
  };

  return (
    <div className="p-4 lernkarten-container">
      <div className="flex justify-between items-center mb-4">
        <h2 className="text-2xl font-bold">Lernkarten</h2>
      </div>
      <div className="create-card-button-container">
        <button onClick={openModal} className="bg-blue-500 text-white p-2 rounded">
          Neue Karte erstellen
        </button>
      </div>

      {isModalOpen && <LernkartenModal addCard={addCard} closeModal={closeModal} />}

      <div className="flex flex-wrap justify-start gap-4">
        {cards.map((card, index) => (
          <div
            key={index}
            className="lernkarte"
            onClick={() => flipCard(index)}
          >
            <p>{flippedCard === index ? card.back : card.front}</p>
            <button
              onClick={(e) => {
                e.stopPropagation();
                deleteCard(index);
              }}
              className="bg-red-500 text-white p-1 rounded mt-2"
            >
              Löschen
            </button>
          </div>
        ))}
      </div>
    </div>
  );
};

export default Lernkarten;