import { useEffect, useState, useRef } from 'react';
import './CustomCursor.css';

export const CustomCursor = () =>
{
    const [position, setPosition] = useState({ x: 0, y: 0 });
    const [isHovering, setIsHovering] = useState(false);
    const [isLightBackground, setIsLightBackground] = useState(true);
    const rafRef = useRef<number>(1);
    const lastCheckTime = useRef(0);

    useEffect(() =>
    {
        let mouseX = 0;
        let mouseY = 0;

        const updatePosition = (e: MouseEvent) =>
        {
            mouseX = e.clientX;
            mouseY = e.clientY;

            if (rafRef.current)
            {
                cancelAnimationFrame(rafRef.current);
            }

            rafRef.current = requestAnimationFrame(() =>
            {
                setPosition({ x: mouseX, y: mouseY });
            });
        };

        const checkInteractive = (target: HTMLElement): boolean =>
        {
            return (
                target.tagName === 'BUTTON' ||
                target.tagName === 'A' ||
                target.tagName === 'INPUT' ||
                target.tagName === 'TEXTAREA' ||
                !!target.closest('button') ||
                !!target.closest('a') ||
                target.classList.contains('clickable') ||
                window.getComputedStyle(target).cursor === 'pointer'
            );
        };

        const checkBackground = (e: MouseEvent) =>
        {
            const now = Date.now();
            if (now - lastCheckTime.current < 100) return;
            lastCheckTime.current = now;

            const target = e.target as HTMLElement;

            setIsHovering(checkInteractive(target));

            const nav = document.querySelector('nav');
            const navRect = nav?.getBoundingClientRect();

            if (navRect && e.clientY < navRect.bottom)
            {
                setIsLightBackground(false);
            }
            else
            {
                setIsLightBackground(true);
            }
        };

        window.addEventListener('mousemove', updatePosition, { passive: true });
        window.addEventListener('mousemove', checkBackground, { passive: true });

        return () =>
        {
            window.removeEventListener('mousemove', updatePosition);
            window.removeEventListener('mousemove', checkBackground);
            if (rafRef.current)
            {
                cancelAnimationFrame(rafRef.current);
            }
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
                willChange: 'left, top',
            }}
        >
            <svg width="32" height="32" viewBox="0 0 32 32" xmlns="http://www.w3.org/2000/svg">
                <circle
                    cx="16"
                    cy="16"
                    r="10"
                    fill="none"
                    stroke={cursorColor}
                    strokeWidth="2.5"
                    className="cursor-outer"
                />
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