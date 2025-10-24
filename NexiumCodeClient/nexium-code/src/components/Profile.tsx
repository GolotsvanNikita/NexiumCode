import { useEffect, useState } from 'react';
import { useParams } from 'react-router-dom';
import axios from 'axios';
import './Profile.css';

interface Certificate
{
    id: number;
    courseId: number;
    courseName: string;
    certificateUrl: string;
    issueDate: string;
}

interface SkillTree
{
    theoryMaster: number;
    theoryMasterRank: number;
    practicePro: number;
    practiceProRank: number;
    quizChampion: number;
    quizChampionRank: number;
    communityStar: number;
    communityStarRank: number;
}

interface UserProfile
{
    id: number;
    username: string;
    email: string;
    rating: number;
    level: number;
    currentXP: number;
    xpToNextLevel: number;
    totalXP: number;
    avatarUrl: string;
    certificates: Certificate[];
    skillTree: SkillTree;
    streak: number;
    achievements: string[];
}

interface ProfileProps
{
    userId: number | null;
    onAvatarUpdate?: (avatarUrl: string) => void;
}

export const Profile: React.FC<ProfileProps> = ({ userId, onAvatarUpdate }) =>
{
    const { userId: profileUserId } = useParams<{ userId: string }>();
    const [profile, setProfile] = useState<UserProfile | null>(null);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState<string | null>(null);
    const [uploading, setUploading] = useState(false);

    const isOwnProfile = userId?.toString() === profileUserId;

    useEffect(() =>
    {
        const fetchProfile = async () =>
        {
            try {
                setLoading(true);
                const id = profileUserId || userId;
                const response = await axios.get(`http://localhost:5064/api/User/profile/${id}`);
                setProfile(response.data);
                setLoading(false);
            }
            catch (err: any)
            {
                console.error('Error loading profile:', err);
                setError('Failed to load profile');
                setLoading(false);
            }
        };

        fetchProfile();
    }, [profileUserId, userId]);

    const handleAvatarUpload = async (e: React.ChangeEvent<HTMLInputElement>) =>
    {
        if (!e.target.files || !e.target.files[0] || !userId) return;

        const file = e.target.files[0];

        if (file.size > 5 * 1024 * 1024)
        {
            alert('File too large. Max 5MB');
            return;
        }

        const allowedTypes = ['image/jpeg', 'image/jpg', 'image/png', 'image/gif'];
        if (!allowedTypes.includes(file.type))
        {
            alert('Invalid file type. Only jpg, jpeg, png, gif allowed');
            return;
        }

        const formData = new FormData();
        formData.append('avatar', file);

        try
        {
            setUploading(true);
            const response = await axios.post(
                `http://localhost:5064/api/User/upload-avatar?userId=${userId}`,
                formData,
                { headers: { 'Content-Type': 'multipart/form-data' } }
            );

            if (profile)
            {
                setProfile({ ...profile, avatarUrl: response.data.avatarUrl });
            }

            if (onAvatarUpdate)
            {
                onAvatarUpdate(response.data.avatarUrl);
            }
        }
        catch (error: any)
        {
            console.error('Error uploading avatar:', error);
            alert(error.response?.data?.Message || 'Failed to upload avatar');
        }
        finally
        {
            setUploading(false);
        }
    };

    const getRankName = (elo: number) =>
    {
        if (elo < 300) return { name: 'Beginner', color: '#8B7355' };
        if (elo < 500) return { name: 'Student', color: '#4A90E2' };
        if (elo < 800) return { name: 'Developer', color: '#50C878' };
        if (elo < 1000) return { name: 'Professional', color: '#9B59B6' };
        if (elo < 2000) return { name: 'Expert', color: '#E67E22' };
        return { name: 'Master', color: '#FFD700' };
    };

    const getSkillColor = (value: number) =>
    {
        if (value < 25) return '#e74c3c';
        if (value < 50) return '#f39c12';
        if (value < 75) return '#3498db';
        return '#2ecc71';
    };

    if (loading)
    {
        return (
            <div className="profile-container">
                <div className="profile-card">
                    <div className="profile-header">
                        <div className="skeleton skeleton-avatar-large"></div>
                        <div className="skeleton skeleton-title"></div>
                        <div className="skeleton skeleton-email"></div>
                    </div>
                    <div className="profile-stats">
                        <div className="stat-item">
                            <div className="skeleton skeleton-stat"></div>
                        </div>
                        <div className="stat-item">
                            <div className="skeleton skeleton-stat"></div>
                        </div>
                    </div>
                </div>
            </div>
        );
    }

    if (error) return <div className="error">{error}</div>;
    if (!profile) return <div className="error">Profile not found</div>;

    const rank = getRankName(profile.rating);
    const xpProgress = (profile.currentXP / profile.xpToNextLevel) * 100;

    const skillIcons =
    {
        theoryMaster: 'http://localhost:5064/images/brain.png',
        practicePro: 'http://localhost:5064/images/arm.png',
        quizChampion: 'http://localhost:5064/images/darts.png',
        communityStar: 'http://localhost:5064/images/message.png',
    };

    return (
        <div className="profile-container">
            <div className="profile-card">
                <div className="profile-header">
                    <div className="profile-avatar-wrapper">
                        <img
                            src={profile.avatarUrl ? `http://localhost:5064${profile.avatarUrl}` : 'http://localhost:5064/images/avatars/default-avatar.png'}
                            alt={profile.username}
                            className="profile-avatar-img"
                        />
                        {isOwnProfile && (
                            <label className="avatar-upload-btn">
                                <input
                                    type="file"
                                    accept="image/jpeg,image/jpg,image/png,image/gif"
                                    onChange={handleAvatarUpload}
                                    disabled={uploading}
                                    style={{ display: 'none' }}
                                />
                                <img src='http://localhost:5064/images/Download.png' className='download' alt="upload" />
                            </label>
                        )}
                    </div>
                    <h1>{profile.username}</h1>
                    <p className="profile-email">{profile.email}</p>

                    {profile.streak > 0 && (
                        <div className="streak-badge">
                            <img src="http://localhost:5064/images/streak-icon.png" alt="Streak" className="streak-icon" />
                            {profile.streak} day streak
                        </div>
                    )}
                </div>

                <div className="profile-main-stats">
                    <div className="elo-card">
                        <div className="elo-label">ELO</div>
                        <div className="elo-value" style={{ color: rank.color }}>
                            {profile.rating}
                        </div>
                        <div className="rank-badge" style={{ background: rank.color }}>
                            {rank.name}
                        </div>
                    </div>

                    <div className="level-card">
                        <div className="level-header">
                            <span className="level-label">Level {profile.level}</span>
                            <span className="level-xp">{profile.currentXP} / {profile.xpToNextLevel} XP</span>
                        </div>
                        <div className="xp-bar">
                            <div className="xp-fill" style={{ width: `${xpProgress}%` }}></div>
                        </div>
                    </div>
                </div>

                <div className="skill-tree-section">
                    <h2>
                        <img src="http://localhost:5064/images/tree.png" alt="Skill Tree" className="section-icon" />
                        Skill Tree
                    </h2>
                    <div className="skill-tree">
                        <div className="skill-branch">
                            <img src={skillIcons.theoryMaster} alt="Theory Master" className="skill-icon" />
                            <div className="skill-info">
                                <div className="skill-name">
                                    Theory
                                    {profile.skillTree.theoryMasterRank > 0 && (
                                        <span className="skill-rank"> ★{profile.skillTree.theoryMasterRank}</span>
                                    )}
                                </div>
                                <div className="skill-bar">
                                    <div
                                        className="skill-fill"
                                        style={{
                                            width: `${profile.skillTree.theoryMaster}%`,
                                            background: getSkillColor(profile.skillTree.theoryMaster),
                                        }}
                                    ></div>
                                </div>
                                <div className="skill-value">{profile.skillTree.theoryMaster}%</div>
                            </div>
                        </div>

                        <div className="skill-branch">
                            <img src={skillIcons.practicePro} alt="Practice Pro" className="skill-icon" />
                            <div className="skill-info">
                                <div className="skill-name">
                                    Practice
                                    {profile.skillTree.practiceProRank > 0 && (
                                        <span className="skill-rank"> ★{profile.skillTree.practiceProRank}</span>
                                    )}
                                </div>
                                <div className="skill-bar">
                                    <div
                                        className="skill-fill"
                                        style={{
                                            width: `${profile.skillTree.practicePro}%`,
                                            background: getSkillColor(profile.skillTree.practicePro),
                                        }}
                                    ></div>
                                </div>
                                <div className="skill-value">{profile.skillTree.practicePro}%</div>
                            </div>
                        </div>

                        <div className="skill-branch">
                            <img src={skillIcons.quizChampion} alt="Quiz Champion" className="skill-icon" />
                            <div className="skill-info">
                                <div className="skill-name">
                                    Quiz
                                    {profile.skillTree.quizChampionRank > 0 && (
                                        <span className="skill-rank"> ★{profile.skillTree.quizChampionRank}</span>
                                    )}
                                </div>
                                <div className="skill-bar">
                                    <div
                                        className="skill-fill"
                                        style={{
                                            width: `${profile.skillTree.quizChampion}%`,
                                            background: getSkillColor(profile.skillTree.quizChampion),
                                        }}
                                    ></div>
                                </div>
                                <div className="skill-value">{profile.skillTree.quizChampion}%</div>
                            </div>
                        </div>

                        <div className="skill-branch">
                            <img src={skillIcons.communityStar} alt="Community Star" className="skill-icon" />
                            <div className="skill-info">
                                <div className="skill-name">
                                    Community
                                    {profile.skillTree.communityStarRank > 0 && (
                                        <span className="skill-rank"> ★{profile.skillTree.communityStarRank}</span>
                                    )}
                                </div>
                                <div className="skill-bar">
                                    <div
                                        className="skill-fill"
                                        style={{
                                            width: `${profile.skillTree.communityStar}%`,
                                            background: getSkillColor(profile.skillTree.communityStar),
                                        }}
                                    ></div>
                                </div>
                                <div className="skill-value">{profile.skillTree.communityStar}%</div>
                            </div>
                        </div>
                    </div>
                </div>

                {profile.achievements && profile.achievements.length > 0 && (
                    <div className="achievements-section">
                        <h2>
                            <img src="http://localhost:5064/images/achievements.png" alt="Achievements" className="section-icon" />
                            Achievements
                        </h2>
                        <div className="achievements-grid">
                            {profile.achievements.map((achievement, index) => (
                                <div key={index} className="achievement-badge">
                                    <img src="http://localhost:5064/images/badge.png" alt={achievement} className="achievement-icon" />
                                    {achievement}
                                </div>
                            ))}
                        </div>
                    </div>
                )}

                {profile.certificates && profile.certificates.length > 0 && (
                    <div className="certificates-section">
                        <h2>
                            <img src="http://localhost:5064/images/certificates-icon.png" alt="Certificates" className="section-icon" />
                            Certificates
                        </h2>
                        <div className="certificates-list">
                            {profile.certificates.map((cert) => (
                                <div key={cert.id} className="certificate-card">
                                    <h3>{cert.courseName}</h3>
                                    <p>Issued: {new Date(cert.issueDate).toLocaleDateString()}</p>
                                    {cert.certificateUrl && (
                                        <a href={cert.certificateUrl} target="_blank" rel="noopener noreferrer">
                                            View Certificate
                                        </a>
                                    )}
                                </div>
                            ))}
                        </div>
                    </div>
                )}
            </div>
        </div>
    );
};