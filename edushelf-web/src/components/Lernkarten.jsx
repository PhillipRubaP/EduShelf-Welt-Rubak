import React, { useState } from 'react';
import LernkartenModal from './LernkartenModal';
import { FaPen, FaTrash } from 'react-icons/fa';
import './Files.css';

const Lernkarten = () => {
    const [cards, setCards] = useState([]);
    const [flippedCard, setFlippedCard] = useState(null);
    const [isModalOpen, setIsModalOpen] = useState(false);
    const [openMenuId, setOpenMenuId] = useState(null);

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

    const toggleMenu = (index) => {
        setOpenMenuId(openMenuId === index ? null : index);
    };

    return (
        <div className="files-container">
            <div className="file-list">
                <div className="file-list-header">
                    <h2>Lernkarten</h2>
                    <button onClick={() => setIsModalOpen(true)} className="add-file-button">+</button>
                </div>
                {isModalOpen && <LernkartenModal addCard={addCard} closeModal={() => setIsModalOpen(false)} />}
                <div className="file-grid">
                    {cards.map((card, index) => (
                        <div key={index} className="file-card lernkarte" onClick={() => flipCard(index)}>
                            <p>{flippedCard === index ? card.back : card.front}</p>
                            <div className="file-card-buttons">
                                <button className="menu-button" onClick={(e) => { e.stopPropagation(); toggleMenu(index); }}>
                                    <div className="menu-icon"></div>
                                    <div className="menu-icon"></div>
                                    <div className="menu-icon"></div>
                                </button>
                                {openMenuId === index && (
                                    <div className="dropdown-menu">
                                        <button onClick={(e) => { e.stopPropagation(); /* handleEdit(index) */ }} title="Edit"><FaPen /></button>
                                        <button onClick={(e) => { e.stopPropagation(); deleteCard(index); }} title="Delete" className="delete-button"><FaTrash /></button>
                                    </div>
                                )}
                            </div>
                        </div>
                    ))}
                </div>
            </div>
        </div>
    );
};

export default Lernkarten;