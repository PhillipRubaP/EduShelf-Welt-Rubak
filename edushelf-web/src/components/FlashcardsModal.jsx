import React, { useState, useEffect } from 'react';
import './FlashcardsModal.css';

const FlashcardsModal = ({ addCard, closeModal, card, updateCard }) => {
    const [front, setFront] = useState('');
    const [back, setBack] = useState('');
    const [tags, setTags] = useState('');

    const isEditing = card !== null;

    useEffect(() => {
        if (isEditing) {
            setFront(card.question);
            setBack(card.answer);
            setTags(card.tags ? card.tags.join(', ') : '');
        }
    }, [card, isEditing]);

    const handleSubmit = (e) => {
        e.preventDefault();
        if (front.trim() !== '' && back.trim() !== '') {
            const tagsArray = tags.split(',').map(tag => tag.trim()).filter(tag => tag);
            const flashcardData = {
                Question: front,
                Answer: back,
                Tags: tagsArray
            };

            if (isEditing) {
                updateCard({
                    ...card,
                    question: flashcardData.Question,
                    answer: flashcardData.Answer,
                    tags: flashcardData.Tags
                });
            } else {
                addCard(flashcardData);
            }
            closeModal();
        }
    };

    return (
        <div className="modal-overlay">
            <div className="modal-container">
                <button className="modal-close-button" onClick={closeModal}>
                    <svg xmlns="http://www.w3.org/2000/svg" className="h-6 w-6" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                        <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M6 18L18 6M6 6l12 12" />
                    </svg>
                </button>

                <div className="modal-header">
                    <h2 className="modal-title">{isEditing ? 'Edit Flashcard' : 'Create New Flashcard'}</h2>
                </div>

                <div className="modal-body">
                    <form onSubmit={handleSubmit} id="flashcard-form">
                        <div className="form-group">
                            <label className="form-label" htmlFor="front">Front (Question)</label>
                            <input
                                type="text"
                                id="front"
                                value={front}
                                onChange={(e) => setFront(e.target.value)}
                                className="form-input"
                                placeholder="Enter question"
                                required
                            />
                        </div>
                        <div className="form-group">
                            <label className="form-label" htmlFor="back">Back (Answer)</label>
                            <textarea
                                id="back"
                                value={back}
                                onChange={(e) => setBack(e.target.value)}
                                className="form-textarea"
                                placeholder="Enter answer"
                                required
                            />
                        </div>
                        <div className="form-group">
                            <label className="form-label" htmlFor="tags">Tags</label>
                            <input
                                type="text"
                                id="tags"
                                value={tags}
                                onChange={(e) => setTags(e.target.value)}
                                className="form-input"
                                placeholder="e.g. Math, Formulas (comma separated)"
                            />
                        </div>
                    </form>
                </div>

                <div className="modal-footer">
                    <button type="button" className="btn btn-secondary" onClick={closeModal}>
                        Cancel
                    </button>
                    <button type="submit" form="flashcard-form" className="btn btn-primary">
                        {isEditing ? 'Update Card' : 'Add Card'}
                    </button>
                </div>
            </div>
        </div>
    );
};

export default FlashcardsModal;