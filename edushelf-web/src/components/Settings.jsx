import React, { useState, useEffect } from 'react';
import './Settings.css';

const Settings = () => {
    const [lightMode, setLightMode] = useState(false);

    useEffect(() => {
        const currentTheme = localStorage.getItem('theme');
        if (currentTheme === 'light') {
            setLightMode(true);
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

    return (
        <div className="settings-container">
            <h2>Einstellungen</h2>
            <div className="settings-option">
                <label htmlFor="light-mode-switch">Light Mode</label>
                <input
                    type="checkbox"
                    id="light-mode-switch"
                    checked={lightMode}
                    onChange={handleLightModeToggle}
                />
            </div>
        </div>
    );
};

export default Settings;