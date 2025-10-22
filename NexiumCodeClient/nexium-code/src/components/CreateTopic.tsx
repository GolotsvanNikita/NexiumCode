import React, { useState } from "react";
import { useNavigate } from "react-router-dom";
import ReactMarkdown from "react-markdown";
import { Prism as SyntaxHighlighter } from "react-syntax-highlighter";
import { materialOceanic } from "react-syntax-highlighter/dist/esm/styles/prism";
import { forumService } from "../services/forumService";
import "./CreateTopic.css";

interface CreateTopicProps
{
    userId: number | null;
}

export const CreateTopic: React.FC<CreateTopicProps> = ({ userId }) =>
{
    const navigate = useNavigate();
    const [title, setTitle] = useState("");
    const [category, setCategory] = useState("JavaScript");
    const [content, setContent] = useState("");
    const [preview, setPreview] = useState(false);
    const [loading, setLoading] = useState(false);
    const [error, setError] = useState<string | null>(null);

    const handlePublish = async () =>
    {
        if (!title.trim() || !content.trim())
        {
            setError("Please fill in all fields");
            return;
        }

        if (!userId)
        {
            setError("You must be logged in");
            return;
        }

        try
        {
            setLoading(true);
            setError(null);

            const response = await forumService.createThread({
                userId,
                title: title.trim(),
                content: content.trim(),
                category,
            });

            navigate(`/forum/${response.threadId}`);
        }
        catch (err)
        {
            console.error("Error creating thread:", err);
            setError("Failed to create thread. Please try again.");
            setLoading(false);
        }
    };

    const insertCodeBlock = () =>
    {
        const textarea = document.getElementById("topic-textarea") as HTMLTextAreaElement;
        if (!textarea) return;

        const start = textarea.selectionStart;
        const end = textarea.selectionEnd;
        const before = content.substring(0, start);
        const after = content.substring(end);
        const codeTemplate = `\n\`\`\`js\n// your code here\n\`\`\`\n`;

        const newContent = before + codeTemplate + after;
        setContent(newContent);

        setTimeout(() =>
        {
            textarea.focus();
            const cursorPosition = start + 8;
            textarea.selectionStart = textarea.selectionEnd = cursorPosition;
        }, 0);
    };

    return (
        <div className="create-topic-container">
            <div className="backButtonDiv">
                <button onClick={() => navigate("/forum")} className="back-button">
                    ← Back
                </button>
            </div>

            <h2>Create New Thread</h2>

            {error && <div className="error-message">{error}</div>}

            <input
                type="text"
                placeholder="Thread title"
                value={title}
                onChange={(e) => setTitle(e.target.value)}
                className="topic-input"
                disabled={loading}
            />

            <select
                value={category}
                onChange={(e) => setCategory(e.target.value)}
                className="topic-select"
                disabled={loading}
            >
                <option>JavaScript</option>
                <option>C#</option>
                <option>HTML/CSS</option>
                <option>Python</option>
            </select>

            <div className="editor-toolbar">
                <button onClick={() => setContent((p) => p + "**bold** ")} disabled={loading}>
                    B
                </button>
                <button onClick={() => setContent((p) => p + "_italic_ ")} disabled={loading}>
                    I
                </button>
                <button onClick={insertCodeBlock} disabled={loading}>{`</>`}</button>
                <button onClick={() => setPreview(!preview)} disabled={loading}>
                    👁 {preview ? "Editor" : "Preview"}
                </button>
            </div>

            {!preview ? (
                <textarea
                    id="topic-textarea"
                    value={content}
                    onChange={(e) => setContent(e.target.value)}
                    className="topic-textarea"
                    placeholder="Enter thread content..."
                    disabled={loading}
                />
            ) : (
                <div className="topic-preview">
                    <ReactMarkdown
                        children={content}
                        components={{
                            code({ className, children })
                            {
                                const match = /language-(\w+)/.exec(className || "");
                                return match ? (
                                    <SyntaxHighlighter
                                        style={materialOceanic}
                                        language={match[1]}
                                        PreTag="div"
                                    >
                                        {String(children).replace(/\n$/, "")}
                                    </SyntaxHighlighter>
                                ) : (
                                    <code className={className}>{children}</code>
                                );
                            },
                        }}
                    />
                </div>
            )}

            <button
                onClick={handlePublish}
                className="publish-button"
                disabled={loading}
            >
                {loading ? "Publishing..." : "Publish"}
            </button>
        </div>
    );
};