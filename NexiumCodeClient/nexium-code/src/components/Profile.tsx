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
    avatarUrl: string;
    certificates: Certificate[];
}

interface ProfileProps
{
    userId: number | null;
    onAvatarUpdate?: (avatarUrl: string) => void; // Добавляем callback
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
            try
            {
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
                {
                    headers: { 'Content-Type': 'multipart/form-data' }
                }
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

    if (loading)
    {
        return (
            <div className="profile-container">
                <div className="profile-card">
                    <div className="profile-header">
                        <div className="skeleton skeleton-avatar-large" style={{width: '120px', height: '120px', borderRadius: '50%', margin: '0 auto 20px'}}></div>
                        <div className="skeleton" style={{width: '200px', height: '32px', margin: '0 auto 10px'}}></div>
                        <div className="skeleton" style={{width: '250px', height: '18px', margin: '0 auto'}}></div>
                    </div>
                    <div className="profile-stats">
                        <div className="stat-item">
                            <div className="skeleton" style={{width: '80px', height: '20px', margin: '0 auto 10px'}}></div>
                            <div className="skeleton" style={{width: '60px', height: '40px', margin: '0 auto'}}></div>
                        </div>
                        <div className="stat-item">
                            <div className="skeleton" style={{width: '100px', height: '20px', margin: '0 auto 10px'}}></div>
                            <div className="skeleton" style={{width: '60px', height: '40px', margin: '0 auto'}}></div>
                        </div>
                    </div>
                </div>
            </div>
        );
    }
    if (error) return <div className="error">{error}</div>;
    if (!profile) return <div className="error">Profile not found</div>;

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
                                <img src='http://localhost:5064/images/Download.png' className='download' alt="download"/>
                            </label>
                        )}
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