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

interface UserProfile
{
    id: number;
    username: string;
    email: string;
    rating: number;
    certificates: Certificate[];
}

interface ProfileProps
{
    userId: number | null;
}

export const Profile: React.FC<ProfileProps> = ({ userId }) =>
{
    const { userId: profileUserId } = useParams<{ userId: string }>();
    const [profile, setProfile] = useState<UserProfile | null>(null);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState<string | null>(null);

    useEffect(() =>
    {
        const fetchProfile = async () =>
        {
            try
            {
                setLoading(true);
                const id = profileUserId || userId;
                const response = await axios.get(`/api/User/profile/${id}`);
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

    if (loading) return <div className="loading">Loading...</div>;
    if (error) return <div className="error">{error}</div>;
    if (!profile) return <div className="error">Profile not found</div>;

    return (
        <div className="profile-container">
            <div className="profile-card">
                <div className="profile-header">
                    <div className="profile-avatar">
                        {profile.username.charAt(0).toUpperCase()}
                    </div>
                    <h1>{profile.username}</h1>
                    <p className="profile-email">{profile.email}</p>
                </div>

                <div className="profile-stats">
                    <div className="stat-item">
                        <div className="stat-label">Rating</div>
                        <div className="stat-value">{profile.rating}</div>
                    </div>
                    <div className="stat-item">
                        <div className="stat-label">Certificates</div>
                        <div className="stat-value">{profile.certificates?.length || 0}</div>
                    </div>
                </div>

                {profile.certificates && profile.certificates.length > 0 && (
                    <div className="certificates-section">
                        <h2>Certificates</h2>
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