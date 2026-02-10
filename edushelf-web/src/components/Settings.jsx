import React, { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import './Settings.css';

const Settings = () => {
    const [lightMode, setLightMode] = useState(false);
    const [animationsEnabled, setAnimationsEnabled] = useState(true);
    const [compactMode, setCompactMode] = useState(false);
    const [soundEffects, setSoundEffects] = useState(true);
    const navigate = useNavigate();

    useEffect(() => {
        const currentTheme = localStorage.getItem('theme');
        if (currentTheme === 'light') {
            setLightMode(true);
        }

        const storedAnimations = localStorage.getItem('animationsEnabled');
        if (storedAnimations !== null) {
            setAnimationsEnabled(storedAnimations === 'true');
        }

        const storedCompact = localStorage.getItem('compactMode');
        if (storedCompact !== null) {
            setCompactMode(storedCompact === 'true');
        }

        const storedSound = localStorage.getItem('soundEffects');
        if (storedSound !== null) {
            setSoundEffects(storedSound === 'true');
        }
    }, []);

    const handleLightModeToggle = () => {
        const newLightMode = !lightMode;
        setLightMode(newLightMode);
        if (newLightMode) {
            document.documentElement.setAttribute('data-theme', 'light');
            localStorage.setItem('theme', 'light');
        } else {
            document.documentElement.removeAttribute('data-theme');
            localStorage.removeItem('theme');
        }
    };

    const handleAnimationsToggle = () => {
        const newValue = !animationsEnabled;
        setAnimationsEnabled(newValue);
        localStorage.setItem('animationsEnabled', String(newValue));
    };

    const handleCompactModeToggle = () => {
        const newValue = !compactMode;
        setCompactMode(newValue);
        localStorage.setItem('compactMode', String(newValue));
        if (newValue) {
            document.body.classList.add('compact-mode');
        } else {
            document.body.classList.remove('compact-mode');
        }
    };

    const handleSoundEffectsToggle = () => {
        const newValue = !soundEffects;
        setSoundEffects(newValue);
        localStorage.setItem('soundEffects', String(newValue));
    };


    const handleClose = () => {
        navigate(-1);
    };

    return (
        <div className="modal-overlay" onClick={handleClose}>
            <div className="modal-container settings-modal" onClick={(e) => e.stopPropagation()}>
                <button className="modal-close-button" onClick={handleClose} aria-label="Close">
                    <svg xmlns="http://www.w3.org/2000/svg" width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
                        <line x1="18" y1="6" x2="6" y2="18"></line>
                        <line x1="6" y1="6" x2="18" y2="18"></line>
                    </svg>
                </button>

                <div className="modal-header">
                    <h2 className="modal-title">Einstellungen</h2>
                    <p className="settings-subtitle">Passe EduShelf an deine Bedürfnisse an</p>
                </div>

                <div className="modal-body settings-body">
                    <div className="settings-section">
                        <h3 className="section-title">Erscheinungsbild</h3>

                        <div className="setting-item">
                            <div className="setting-info">
                                <span className="setting-label">Light Mode</span>
                                <span className="setting-desc">Helles Design für bessere Lesbarkeit am Tag</span>
                            </div>
                            <label className="toggle-switch">
                                <input
                                    type="checkbox"
                                    checked={lightMode}
                                    onChange={handleLightModeToggle}
                                />
                                <span className="slider round"></span>
                            </label>
                        </div>

                        <div className="setting-item">
                            <div className="setting-info">
                                <span className="setting-label">Kompaktmodus</span>
                                <span className="setting-desc">Mehr Inhalt auf weniger Platz</span>
                            </div>
                            <label className="toggle-switch">
                                <input
                                    type="checkbox"
                                    checked={compactMode}
                                    onChange={handleCompactModeToggle}
                                />
                                <span className="slider round"></span>
                            </label>
                        </div>
                    </div>

                    <div className="settings-section">
                        <h3 className="section-title">Erlebnis</h3>

                        <div className="setting-item">
                            <div className="setting-info">
                                <span className="setting-label">Animationen</span>
                                <span className="setting-desc">Visuelle Effekte für weichere Übergänge</span>
                            </div>
                            <label className="toggle-switch">
                                <input
                                    type="checkbox"
                                    checked={animationsEnabled}
                                    onChange={handleAnimationsToggle}
                                />
                                <span className="slider round"></span>
                            </label>
                        </div>

                        <div className="setting-item">
                            <div className="setting-info">
                                <span className="setting-label">Soundeffekte</span>
                                <span className="setting-desc">Töne bei Interaktionen abspielen</span>
                            </div>
                            <label className="toggle-switch">
                                <input
                                    type="checkbox"
                                    checked={soundEffects}
                                    onChange={handleSoundEffectsToggle}
                                />
                                <span className="slider round"></span>
                            </label>
                        </div>
                    </div>
                </div>
                <div className="modal-footer">
                    <span className="version-info">Version 1.0.2</span>
                </div>
            </div>
        </div>
    );
};

export default Settings;