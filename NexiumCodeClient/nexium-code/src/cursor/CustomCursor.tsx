import { useEffect, useState } from 'react';
import './CustomCursor.css';

export const CustomCursor = () => {
    const [position, setPosition] = useState({ x: 0, y: 0 });
    const [isHovering, setIsHovering] = useState(false);
    const [isLightBackground, setIsLightBackground] = useState(true);

    useEffect(() => {
        const updatePosition = (e: MouseEvent) => {
            setPosition({ x: e.clientX, y: e.clientY });
        };

        const checkBackground = (e: MouseEvent) => {
            let target = e.target as HTMLElement;

            // Проверяем, это кнопка, ссылка или кликабельный элемент
            if (
                target.tagName === 'BUTTON' ||
                target.tagName === 'A' ||
                target.closest('button') ||
                target.closest('a') ||
                target.classList.contains('lesson-nav-item') ||
                target.closest('.lesson-nav-item') ||
                target.classList.contains('clickable') ||
                target.style.cursor === 'pointer' ||
                window.getComputedStyle(target).cursor === 'pointer'
            ) {
                setIsHovering(true);
            } else {
                setIsHovering(false);
            }

            // Ищем первый элемент с реальным фоном, игнорируя текстовые элементы
            let currentElement: HTMLElement | null = target;
            let bgColor = 'rgba(0, 0, 0, 0)';

            while (currentElement && currentElement !== document.body) {
                const computedStyle = window.getComputedStyle(currentElement);
                const bg = computedStyle.backgroundColor;

                // Проверяем, не прозрачный ли фон
                if (bg && bg !== 'rgba(0, 0, 0, 0)' && bg !== 'transparent') {
                    bgColor = bg;
                    break;
                }

                currentElement = currentElement.parentElement;
            }

            // Если не нашли фон, используем body
            if (bgColor === 'rgba(0, 0, 0, 0)') {
                bgColor = window.getComputedStyle(document.body).backgroundColor;
            }

            // Парсим RGB значения
            const rgb = bgColor.match(/\d+/g);
            if (rgb) {
                const r = parseInt(rgb[0]);
                const g = parseInt(rgb[1]);
                const b = parseInt(rgb[2]);

                // Вычисляем яркость (формула относительной яркости)
                const brightness = (r * 299 + g * 587 + b * 114) / 1000;

                // Если яркость больше 128 - светлый фон, иначе темный
                setIsLightBackground(brightness > 128);
            }
        };

        window.addEventListener('mousemove', updatePosition);
        window.addEventListener('mousemove', checkBackground);
        window.addEventListener('mouseover', checkBackground);

        return () => {
            window.removeEventListener('mousemove', updatePosition);
            window.removeEventListener('mousemove', checkBackground);
            window.removeEventListener('mouseover', checkBackground);
        };
    }, []);

    const cursorColor = isHovering
        ? (isLightBackground ? '#3a3a3a' : '#ffffff')
        : (isLightBackground ? '#1a1a1a' : '#e8e3db');

    return (
        <div
            className={`custom-cursor ${isHovering ? 'hovering' : ''}`}
            style={{
                left: `${position.x}px`,
                top: `${position.y}px`,
            }}
        >
            <svg width="32" height="32" viewBox="0 0 32 32" xmlns="http://www.w3.org/2000/svg">
                {/* Внешний круг */}
                <circle
                    cx="16"
                    cy="16"
                    r="10"
                    fill="none"
                    stroke={cursorColor}
                    strokeWidth="2.5"
                    className="cursor-outer"
                />

                {/* Внутренний акцент */}
                <circle
                    cx="16"
                    cy="16"
                    r="6"
                    fill="none"
                    stroke={cursorColor}
                    strokeWidth="1"
                    opacity="0.4"
                    className="cursor-middle"
                />

                {/* Центральная точка */}
                <circle
                    cx="16"
                    cy="16"
                    r="2.5"
                    fill={cursorColor}
                    className="cursor-center"
                />
            </svg>
        </div>
    );
};