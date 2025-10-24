import React, { useState, useEffect } from "react";
import "./ForumThread.css";
import { useNavigate, useParams } from "react-router-dom";
import { PrismLight as SyntaxHighlighter } from "react-syntax-highlighter";
import { vscDarkPlus } from "react-syntax-highlighter/dist/esm/styles/prism";
import javascript from "react-syntax-highlighter/dist/esm/languages/prism/javascript";
import csharp from "react-syntax-highlighter/dist/esm/languages/prism/csharp";
import python from "react-syntax-highlighter/dist/esm/languages/prism/python";
import markup from "react-syntax-highlighter/dist/esm/languages/prism/markup"; 
import { forumService, type Thread, type Reply } from "../services/forumService";
import photoIcon from "/photo.png";

SyntaxHighlighter.registerLanguage('javascript', javascript);
SyntaxHighlighter.registerLanguage('csharp', csharp);
SyntaxHighlighter.registerLanguage('python', python);
SyntaxHighlighter.registerLanguage('html', markup);
SyntaxHighlighter.registerLanguage('css', markup);

interface ForumThreadProps {
    userId: number | null;
}

interface CommentItemProps {
    comment: Reply;
    onReply: (parentId: number, text: string) => void;
    formatDate: (date: string) => string;
    userId: number | null;
}

const parseCommentText = (text: string) => {
    if (!text) return null;
    

    const cleanedText = text
        .replace(/\u00A0/g, ' ') 
        .replace(/\r/g, '');
    const regex = /(```(\w*)\n([\s\S]*?)\n```)|(!\[.*?\]\((.*?)\))|([\s\S]+?)(?=```|!\[.*?\]\(.*?\)|$)/gs;

    const parts = Array.from(cleanedText.matchAll(regex)); 

    return parts.map((match, index) => {
        const [
            fullMatch, 
            codeBlock, 
            lang, 
            codeContent, 
            imageBlock, 
            imageUrl, 
            textContent
        ] = match;

        if (codeBlock) {
            let language = lang?.toLowerCase();
            const supportedLangs = ['javascript', 'csharp', 'python', 'markup']; 
            if (!language || !supportedLangs.includes(language)) {
                language = 'text'; 
            }
            
            return (
                <SyntaxHighlighter
                    key={index}
                    language={language === 'c#' ? 'csharp' : language} 
                    style={vscDarkPlus}
                    PreTag="div"
                    wrapLongLines={true}
                    customStyle={{
                        borderRadius: "10px",
                        background: "#1e1e1e",
                        padding: "14px",
                        fontSize: "14px",
                        lineHeight: "1.5",
                        margin: "10px 0",
                    }}
                >
                    {codeContent.trimEnd()}
                </SyntaxHighlighter>
            );
        }

        if (imageBlock) {
            const imageMatch = /!\[(.*?)\]\((.*?)\)/.exec(imageBlock);
            if (imageMatch) {
                const [, alt, url] = imageMatch;
                return (
                    <img
                        key={index}
                        src={url}
                        alt={alt || "image"}
                        style={{
                            maxWidth: "100%",
                            borderRadius: "8px",
                            margin: "10px 0",
                            display: "block",
                        }}
                    />
                );
            }
        }

        if (textContent) {

            const withImages = textContent.split(/(!\[.*?\]\((.*?)\))/g).map((segment, i) => {
                const match = /!\[(.*?)\]\((.*?)\)/.exec(segment);
                if (match) {
                    const [, alt, url] = match;
                    return (
                        <img
                            key={`text-img-${i}`}
                            src={url}
                            alt={alt || "image"}
                            style={{
                                maxWidth: "100%",
                                borderRadius: "8px",
                                margin: "10px 0",
                                display: "block",
                            }}
                        />
                    );
                }
                return <span key={`text-span-${i}`}>{segment}</span>;
            });

            return (
                <p key={index} style={{ whiteSpace: "pre-wrap", margin: "6px 0" }}>
                    {withImages}
                </p>
            );
        }

        return null;
    }).filter(Boolean);
};


const ThreadContentSkeleton = () => (

    <div className="thread-container">
        <div className="skeleton skeleton-back-btn" style={{width: '100px', height: '32px', marginBottom: '20px'}}></div>

        <div className="topic-header">
            <div style={{flex: 1}}>
                <div className="skeleton skeleton-topic-title" style={{width: '60%', height: '28px', marginBottom: '8px'}}></div>
                <div className="skeleton skeleton-topic-category" style={{width: '120px', height: '24px'}}></div>
            </div>
            <div className="skeleton skeleton-topic-status" style={{width: '80px', height: '32px'}}></div>
        </div>

        <div className="thread-content">
            <div className="thread-author-info">
                <div className="skeleton skeleton-avatar" style={{width: '50px', height: '50px', borderRadius: '50%'}}></div>
                <div style={{flex: 1}}>
                    <div className="skeleton" style={{width: '150px', height: '18px', marginBottom: '6px'}}></div>
                    <div className="skeleton" style={{width: '100px', height: '14px'}}></div>
                </div>
            </div>
            <div className="skeleton" style={{width: '100%', height: '100px', marginTop: '15px'}}></div>
        </div>

        <div className="comments-section">
            <div className="skeleton" style={{width: '150px', height: '24px', marginBottom: '20px'}}></div>
            {[1, 2, 3].map((n) => (
                <div key={n} className="comment-card" style={{marginBottom: '15px'}}>
                    <div className="comment-header">
                        <div className="skeleton skeleton-avatar" style={{width: '40px', height: '40px', borderRadius: '50%'}}></div>
                        <div style={{flex: 1}}>
                            <div className="skeleton" style={{width: '120px', height: '16px', marginBottom: '6px'}}></div>
                            <div className="skeleton" style={{width: '80px', height: '12px'}}></div>
                        </div>
                    </div>
                    <div className="skeleton" style={{width: '100%', height: '60px', marginTop: '12px'}}></div>
                </div>
            ))}
        </div>
    </div>
);

export const ForumThread: React.FC<ForumThreadProps> = ({ userId }) =>
{
    const navigate = useNavigate();
    const { id } = useParams<{ id: string }>();
    const [thread, setThread] = useState<Thread | null>(null);
    const [newComment, setNewComment] = useState("");
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState<string | null>(null);
    const [submitting, setSubmitting] = useState(false);

    useEffect(() =>
    {
        loadThread();
    }, [id]);

    const loadThread = async () =>
    {
        if (!id) return;

        try
        {
            setLoading(true);
            setError(null);
            const data = await forumService.getThread(parseInt(id));
            console.log("Содержимое темы (Thread Content):", data.content);
            setThread(data);
        }
        catch (err)
        {
            console.error("Error loading thread:", err);
            setError("Failed to load thread");
        }
        finally
        {
            setLoading(false);
        }
    };

    const addComment = async () =>
    {
        if (!newComment.trim() || !userId || !thread) return;

        try
        {
            setSubmitting(true);
            await forumService.createReply(thread.id,
            {
                userId,
                content: newComment.trim(),
            });
            setNewComment("");
            await loadThread();
        }
        catch (err)
        {
            console.error("Error adding comment:", err);
            alert("Failed to add comment");
        }
        finally
        {
            setSubmitting(false);
        }
    };

    const addReply = async (parentId: number, text: string) =>
    {
        if (!text.trim() || !userId || !thread) return;

        try
        {
            await forumService.createReply(thread.id,
            {
                userId,
                content: text.trim(),
                parentReplyId: parentId,
            });
            await loadThread();
        }
        catch (err)
        {
            console.error("Error adding reply:", err);
            alert("Failed to add reply");
        }
    };

    const handleResolve = async () =>
    {
        if (!thread || !userId || thread.userId !== userId) return;

        try
        {
            await forumService.markAsResolved(thread.id, userId);
            await loadThread();
        }
        catch (err)
        {
            console.error("Error marking thread as resolved:", err);
            alert("Failed to mark thread as resolved");
        }
    };

    const handleDelete = async () =>
    {
        if (!thread || !userId || thread.userId !== userId) return;

        if (!confirm("Are you sure you want to delete this thread?")) return;

        try
        {
            await forumService.deleteThread(thread.id, userId);
            navigate("/forum");
        }
        catch (err)
        {
            console.error("Error deleting thread:", err);
            alert("Failed to delete thread");
        }
    };

    const formatDate = (dateString: string) =>
    {
        const date = new Date(dateString);
        const now = new Date();
        const diffMs = now.getTime() - date.getTime();
        const diffMins = Math.floor(diffMs / 60000);
        const diffHours = Math.floor(diffMs / 3600000);
        const diffDays = Math.floor(diffMs / 86400000);

        if (diffMins < 60) return `${diffMins} min ago`;
        if (diffHours < 24) return `${diffHours} hours ago`;
        if (diffDays < 7) return `${diffDays} days ago`;
        return date.toLocaleDateString("en-US");
    };

    const renderReplies = (replies: Reply[] | undefined) =>
    {
        if (!replies || replies.length === 0) return null;

        return replies.map((reply) => (
            <CommentItem
                key={reply.id}
                comment={reply}
                onReply={addReply}
                formatDate={formatDate}
                userId={userId}
            />
        ));
    };

    if (loading)
    {
        return <ThreadContentSkeleton />;
    }

    if (error || !thread)
    {
        return (
            <div className="thread-container">
                <button className="back-button" onClick={() => navigate("/forum")}>
                    ← Back to threads
                </button>
                <div className="error-message">{error || "Thread not found"}</div>
            </div>
        );
    }

    return (
        <div className="thread-container">
            <button className="back-button" onClick={() => navigate("/forum")}>
                ← Back to threads
            </button>

            <div className="topic-header">
                <div>
                    <h2 className="topic-title">{thread.title}</h2>
                    <span className="topic-category">Category: {thread.category}</span>
                </div>
                <div className="topic-actions">
                    <div
                        className={`topic-status ${
                            thread.isResolved ? "solved" : "open"
                        }`}
                    >
                        {thread.isResolved ? "Solved" : "Open"}
                    </div>
                    {userId === thread.userId && !thread.isResolved && (
                        <button
                            className="resolve-button"
                            onClick={(e) =>
                            {
                                e.stopPropagation();
                                if (confirm("Mark this thread as solved?"))
                                {
                                    handleResolve();
                                }
                            }}
                            title="Mark as solved"
                        >
                            ✓
                        </button>
                    )}
                    {userId === thread.userId && (
                        <button
                            className="delete-button"
                            onClick={(e) =>
                            {
                                e.stopPropagation();
                                if (confirm("Are you sure you want to delete this thread?")) {
                                    handleDelete();
                                }
                            }}
                            title="Delete thread"
                        >
                            🗑️
                        </button>
                    )}
                </div>
            </div>

            <div className="thread-content">
                <div className="thread-author-info">
                    <img
                        src={thread.avatarUrl ? `http://localhost:5064${thread.avatarUrl}` : 'http://localhost:5064/images/avatars/default-avatar.png'}
                        alt={thread.username}
                        className="thread-author-avatar"
                        onClick={() => navigate(`/profile/${thread.userId}`)}
                    />
                    <div className="thread-author-details">
                        <span className="author-name" onClick={() => navigate(`/profile/${thread.userId}`)}>{thread.username}</span>
                        <span className="thread-date">{formatDate(thread.createdAt)}</span>
                    </div>
                </div>
                <div className="thread-text">{parseCommentText(thread.content)}</div>
            </div>

            <div className="comments-section">
                <h3>Replies ({thread.replyCount})</h3>
                <div className="comments-list">{renderReplies(thread.replies)}</div>
            </div>

            <div className="new-comment">
                <div className="input-wrapper">
                    <div className="comment-actions">
                    </div>
                                     <button className="button-image"
                            onClick={() => document.getElementById("replyImageInput")?.click()}
                            disabled={submitting}
                        >
                            <img className="photoIcon"src={photoIcon}/>
                        </button>
                        <input
                            type="file"
                            id="replyImageInput"
                            accept="image/*"
                            style={{ display: "none" }}
                            onChange={async (e) => {
                                const file = e.target.files?.[0];
                                if (!file) return;
                                try {
                                    const url = await forumService.uploadImage(file);
                                    setNewComment((p) => p + `\n![](${url})\n`);
                                } catch {
                                    alert("Image upload failed");
                                }
                            }}
                        />
          <textarea
              placeholder='Write a comment... (use ```lang\ncode\n``` for code blocks)' 
              value={newComment}
              onChange={(e) => setNewComment(e.target.value)}
              className="comment-textarea"
              disabled={submitting}         
          ></textarea>
                    <div className="comment-actions">
                        <button
                            onClick={addComment}
                            className="comment-button"
                            disabled={submitting || !newComment.trim()}
                        >
                            {submitting ? "Sending..." : "Send"}
                        </button>
                    </div>
                </div>
            </div>
        </div>
    );
};

const CommentItem: React.FC<CommentItemProps> = ({
    comment,
    onReply,
    formatDate,
     userId,
     }) => {
    const [isReplying, setIsReplying] = useState(false);
    const [replyText, setReplyText] = useState("");
    const navigate = useNavigate();
    const handleReply = () => {
        if (!replyText.trim()) return;
        onReply(comment.id, replyText);
        setReplyText("");
        setIsReplying(false);
    };

    return (
        <div className="comment">
            <div className="comment-card">
                <div className="comment-header">
                    <img
                        src={comment.avatarUrl ? `http://localhost:5064${comment.avatarUrl}` : 'http://localhost:5064/images/avatars/default-avatar.png'}
                        alt={comment.username}
                        className="comment-avatar"
                        onClick={() => navigate(`/profile/${comment.userId}`)}
                    />
                    <div className="comment-info">
                        <span
                            className="comment-author"
                            onClick={() => navigate(`/profile/${comment.userId}`)}
                        >
                            {comment.username}
                        </span>
                        <span className="comment-date">{formatDate(comment.createdAt)}</span>
                    </div>
                </div>
                <div className="comment-text">{parseCommentText(comment.content)}</div>
                {userId && (
                    <button
                        className="reply-button"
                        onClick={() => setIsReplying(!isReplying)}
                    >
                        Reply
                    </button>
                )}

                {isReplying && (
                    <div className="reply-box">
                        <textarea
                            className="comment-textarea"
                            placeholder="Write a reply..."
                            value={replyText}
                            onChange={(e) => setReplyText(e.target.value)}
                        />
                        <div className="reply-actions">
                            <button onClick={handleReply} className="comment-button">
                                Send
                            </button>
                            <button
                                onClick={() => setIsReplying(false)}
                                className="cancel-button"
                            >
                                Cancel
                            </button>
                        </div>
                    </div>
                )}
            </div>

            {comment.childReplies && comment.childReplies.length > 0 && (
                <div className="replies">
                    {comment.childReplies.map((childReply) => (
                        <CommentItem
                            key={childReply.id}
                            comment={childReply}
                            onReply={onReply}
                            formatDate={formatDate}
                            userId={userId}
                        />
                    ))}
                </div>
            )}
        </div>
    );
};