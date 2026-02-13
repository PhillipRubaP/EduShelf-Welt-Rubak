import React, { useState, useEffect } from 'react';
import { getTags, getFlashcardsByTag } from '../services/api';
import './FlashcardReviewModal.css';

const FlashcardReviewModal = ({ closeModal }) => {
    const [tags, setTags] = useState([]);
    const [selectedTag, setSelectedTag] = useState('');
    const [cards, setCards] = useState([]);
    const [unseenCards, setUnseenCards] = useState([]);
    const [currentCardIndex, setCurrentCardIndex] = useState(0);
    const [isFlipped, setIsFlipped] = useState(false);
    const [reviewStarted, setReviewStarted] = useState(false);

    useEffect(() => {
        const fetchTags = async () => {
            const fetchedTags = await getTags();
            setTags(fetchedTags);
        };
        fetchTags();
    }, []);

    const handleTagChange = (e) => {
        setSelectedTag(e.target.value);
    };

    const startReview = async () => {
        if (selectedTag) {
            try {
                // Fetch up to 100 cards for review
                const result = await getFlashcardsByTag(selectedTag, 1, 100);

                let fetchedCards = [];
                if (result && result.items) {
                    fetchedCards = result.items;
                } else if (Array.isArray(result)) {
                    fetchedCards = result;
                }

                if (fetchedCards.length === 0) {
                    alert("No flashcards found for this tag.");
                    return;
                }

                setCards(fetchedCards);
                setUnseenCards(fetchedCards);
                setCurrentCardIndex(0);
                setIsFlipped(false);
                setReviewStarted(true);
            } catch (error) {
                console.error("Error starting review:", error);
                alert("Failed to load flashcards. Please try again.");
            }
        }
    };

    const handleKnow = () => {
        const newUnseenCards = unseenCards.filter((_, index) => index !== currentCardIndex);
        setUnseenCards(newUnseenCards);

        if (newUnseenCards.length === 0) {
            alert('Congratulations! You have reviewed all cards.');
            closeModal();
        } else {
            setCurrentCardIndex(currentCardIndex % newUnseenCards.length);
            setIsFlipped(false);
        }
    };

    const handleNotKnow = () => {
        setCurrentCardIndex((currentCardIndex + 1) % unseenCards.length);
        setIsFlipped(false);
    };

    const currentCard = unseenCards[currentCardIndex];

    return (
        <div className="modal-backdrop">
            <div className="modal-content">
                <div className="modal-close-button" onClick={closeModal}>
                    <svg xmlns="http://www.w3.org/2000/svg" className="h-6 w-6" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                        <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M6 18L18 6M6 6l12 12" />
                    </svg>
                </div>
                {!reviewStarted ? (
                    <div className="tag-selection">
                        <h2>Select a Tag to Review</h2>
                        <select onChange={handleTagChange} value={selectedTag}>
                            <option value="">Select a tag</option>
                            {tags.map(tag => (
                                <option key={tag.id} value={tag.id}>{tag.name}</option>
                            ))}
                        </select>
                        <button onClick={startReview} disabled={!selectedTag}>Start Review</button>
                    </div>
                ) : currentCard ? (
                    <div>
                        <div className={`flashcard-review-scene ${isFlipped ? 'flipped' : ''}`} onClick={() => setIsFlipped(!isFlipped)}>
                            <div className="flashcard-review-container">
                                <div className="flashcard-face flashcard-front">
                                    <p>{currentCard.question}</p>
                                </div>
                                <div className="flashcard-face flashcard-back">
                                    <p>{currentCard.answer}</p>
                                </div>
                            </div>
                        </div>
                        <div className="review-buttons">
                            <button onClick={handleKnow}>Knew</button>
                            <button onClick={handleNotKnow}>Didn't Know</button>
                        </div>
                    </div>
                ) : (
                    <p>No cards to review for this tag.</p>
                )}
            </div>
        </div>
    );
};

export default FlashcardReviewModal;