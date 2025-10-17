import React, { useState, useEffect } from 'react';
import LernkartenModal from './LernkartenModal';
import { getFlashcards, createFlashcard, deleteFlashcard } from '../services/api';
import { FaPen, FaTrash } from 'react-icons/fa';
import './Files.css';

const Lernkarten = () => {
    const [cards, setCards] = useState([]);
    const [flippedCard, setFlippedCard] = useState(null);
    const [isModalOpen, setIsModalOpen] = useState(false);
    const [openMenuId, setOpenMenuId] = useState(null);

    useEffect(() => {
        const fetchFlashcards = async () => {
            const fetchedCards = await getFlashcards();
            setCards(fetchedCards);
        };
        fetchFlashcards();
    }, []);

    const addCard = async (card) => {
        const newCard = await createFlashcard(card);
        setCards([...cards, newCard]);
    };

    const deleteCard = async (id) => {
        await deleteFlashcard(id);
        setCards(cards.filter(card => card.id !== id));
    };

    const flipCard = (id) => {
        setFlippedCard(flippedCard === id ? null : id);
    };

    const toggleMenu = (id) => {
        setOpenMenuId(openMenuId === id ? null : id);
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
                    {Array.isArray(cards) && cards.map((card, index) => (
                        <div key={card.id || index} className="file-card lernkarte" onClick={() => flipCard(card.id)}>
                            <p>{flippedCard === card.id ? card.answer : card.question}</p>
                            <div className="file-card-buttons">
                                <button className="menu-button" onClick={(e) => { e.stopPropagation(); toggleMenu(card.id); }}>
                                    <div className="menu-icon"></div>
                                    <div className="menu-icon"></div>
                                    <div className="menu-icon"></div>
                                </button>
                                {openMenuId === card.id && (
                                    <div className="dropdown-menu">
                                        <button onClick={(e) => { e.stopPropagation(); /* handleEdit(card.id) */ }} title="Edit"><FaPen /></button>
                                        <button onClick={(e) => { e.stopPropagation(); deleteCard(card.id); }} title="Delete" className="delete-button"><FaTrash /></button>
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