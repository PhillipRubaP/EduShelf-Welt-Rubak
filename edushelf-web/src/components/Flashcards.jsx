import React, { useState, useEffect } from 'react';
import FlashcardsModal from './FlashcardsModal';
import FlashcardReviewModal from './FlashcardReviewModal';
import { getFlashcards, createFlashcard, deleteFlashcard, updateFlashcard } from '../services/api';
import { FaPen, FaTrash } from 'react-icons/fa';
import './Files.css';
import './Flashcards.css';

const Flashcards = () => {
    const [cards, setCards] = useState([]);
    const [flippedCards, setFlippedCards] = useState(new Set());
    const [isModalOpen, setIsModalOpen] = useState(false);
    const [isReviewModalOpen, setIsReviewModalOpen] = useState(false);
    const [openMenuId, setOpenMenuId] = useState(null);
    const [editingCard, setEditingCard] = useState(null);

    // Pagination State
    const [currentPage, setCurrentPage] = useState(1);
    const [totalPages, setTotalPages] = useState(1);
    const pageSize = 10;

    useEffect(() => {
        const fetchFlashcards = async () => {
            try {
                const result = await getFlashcards(currentPage, pageSize);
                // Handle PagedResult
                if (result && result.items) {
                    setCards(result.items);
                    setTotalPages(result.totalPages);
                } else if (Array.isArray(result)) {
                    // Fallback if API returns array (shouldn't happen with updated backend)
                    setCards(result);
                }
            } catch (error) {
                console.error("Failed to fetch flashcards", error);
            }
        };
        fetchFlashcards();
    }, [currentPage]); // Re-fetch when page changes

    const addCard = async (card) => {
        const newCard = await createFlashcard(card);
        setCards([...cards, newCard]);
    };

    const updateCard = async (card) => {
        await updateFlashcard(card);
        const fetchedCards = await getFlashcards();
        setCards(fetchedCards);
    };

    const deleteCard = async (id) => {
        await deleteFlashcard(id);
        setCards(cards.filter(card => card.id !== id));
    };

    const handleEdit = (card) => {
        setEditingCard(card);
        setIsModalOpen(true);
    };

    const flipCard = (id) => {
        setFlippedCards(prevFlippedCards => {
            const newFlippedCards = new Set(prevFlippedCards);
            if (newFlippedCards.has(id)) {
                newFlippedCards.delete(id);
            } else {
                newFlippedCards.add(id);
            }
            return newFlippedCards;
        });
    };

    const toggleMenu = (id) => {
        setOpenMenuId(openMenuId === id ? null : id);
    };

    return (
        <div className="files-container">
            <div className="file-list">
                <div className="file-list-header">
                    <h2>Flashcards</h2>
                    <button onClick={() => setIsReviewModalOpen(true)} className="add-file-button review-button">Review</button>
                    <button onClick={() => { setEditingCard(null); setIsModalOpen(true); }} className="add-file-button">+</button>
                </div>
                {isModalOpen && <FlashcardsModal addCard={addCard} closeModal={() => setIsModalOpen(false)} card={editingCard} updateCard={updateCard} />}
                {isReviewModalOpen && <FlashcardReviewModal closeModal={() => setIsReviewModalOpen(false)} />}
                <div className="flashcards-grid">
                    {Array.isArray(cards) && cards.map((card) => (
                        <div key={card.id} className={`flashcard-scene ${flippedCards.has(card.id) ? 'flipped' : ''}`} style={{ zIndex: openMenuId === card.id ? 100 : 'auto' }} onClick={() => flipCard(card.id)}>
                            <div className="flashcard-container">
                                <div className="flashcard-face flashcard-front">
                                    <p>{card.question}</p>
                                </div>
                                <div className="flashcard-face flashcard-back">
                                    <p>{card.answer}</p>
                                </div>
                            </div>
                            <div className="file-card-buttons">
                                <button className="menu-button" onClick={(e) => { e.stopPropagation(); toggleMenu(card.id); }}>
                                    <div className="menu-icon"></div>
                                    <div className="menu-icon"></div>
                                    <div className="menu-icon"></div>
                                </button>
                                {openMenuId === card.id && (
                                    <div className="dropdown-menu" key={`menu-${card.id}`}>
                                        <button onClick={(e) => { e.stopPropagation(); handleEdit(card); }} title="Edit"><FaPen /></button>
                                        <button onClick={(e) => { e.stopPropagation(); deleteCard(card.id); }} title="Delete" className="delete-button"><FaTrash /></button>
                                    </div>
                                )}
                            </div>
                        </div>
                    ))}
                </div>

                {/* Pagination Controls */}
                {totalPages > 1 && (
                    <div className="pagination-controls">
                        <button
                            onClick={() => setCurrentPage(prev => Math.max(prev - 1, 1))}
                            disabled={currentPage === 1}
                            className="pagination-button"
                        >
                            Previous
                        </button>
                        <span className="pagination-info">Page {currentPage} of {totalPages}</span>
                        <button
                            onClick={() => setCurrentPage(prev => Math.min(prev + 1, totalPages))}
                            disabled={currentPage === totalPages}
                            className="pagination-button"
                        >
                            Next
                        </button>
                    </div>
                )}

            </div>
        </div>
    );
};

export default Flashcards;