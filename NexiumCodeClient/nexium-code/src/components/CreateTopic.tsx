import React, { useState } from "react";
import { useNavigate } from "react-router-dom";
import ReactMarkdown from "react-markdown";
import { Prism as SyntaxHighlighter } from "react-syntax-highlighter";
import { materialOceanic } from "react-syntax-highlighter/dist/esm/styles/prism";
import { forumService } from "../services/forumService";
import "./CreateTopic.css";
import photoIcon from "/photo.png";

import Editor from "react-simple-code-editor";
import Prism from "prismjs";
import "prismjs/components/prism-javascript";
import "prismjs/components/prism-csharp";
import "prismjs/components/prism-python";
import "prismjs/components/prism-markup";
import "prismjs/themes/prism-okaidia.css";

interface CreateTopicProps {
  userId: number | null;
}

export const CreateTopic: React.FC<CreateTopicProps> = ({ userId }) => {
  const navigate = useNavigate();
  const [title, setTitle] = useState("");
  const [category, setCategory] = useState("JavaScript");
  const [content, setContent] = useState("");
  const [preview, setPreview] = useState(false);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const [isCodeMode, setIsCodeMode] = useState(false);

  const handlePublish = async () => {
    if (!title.trim() || !content.trim()) {
      setError("Please fill in all fields");
      return;
    }

    if (!userId) {
      setError("You must be logged in");
      return;
    }

    try {
      setLoading(true);
      setError(null);

      const response = await forumService.createThread({
        userId,
        title: title.trim(),
        content: content.trim(),
        category,
      });

      navigate(`/forum/${response.threadId}`);
    } catch (err) {
      console.error("Error creating thread:", err);
      setError("Failed to create thread. Please try again.");
      setLoading(false);
    }
  };

  const insertCodeBlock = (category: string) => {
    setIsCodeMode(true);
    setCategory(category);
    setContent("// your code here");
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
        <option>CSharp</option>
        <option>HTML/CSS</option>
        <option>Python</option>
        <option>Other</option>
      </select>

      <div className="editor-toolbar">
        <button onClick={() => setContent((p) => p + "**bold** ")} disabled={loading}>
          B
        </button>
        <button onClick={() => setContent((p) => p + "_italic_ ")} disabled={loading}>
          I
        </button>
        <button onClick={() => insertCodeBlock(category)} disabled={loading}>{`</>`}</button>

        <button onClick={() => document.getElementById("fileInput")?.click()} disabled={loading}>
          <img className="photoIcon" src={photoIcon} />
        </button>
        <input
          type="file"
          id="fileInput"
          accept="image/*"
          style={{ display: "none" }}
          onChange={async (e) => {
            const file = e.target.files?.[0];
            if (!file) return;
            try {
              const url = await forumService.uploadImage(file);
              setContent((p) => p + `\n![](${url})\n`);
            } catch {
              alert("Failed to upload image");
            }
          }}
        />

        <button onClick={() => setPreview(!preview)} disabled={loading}>
          👁 {preview ? "Editor" : "Preview"}
        </button>
      </div>

      {!preview ? (
        <>
          {isCodeMode ? (
            <div className="code-editor-wrapper">
              <Editor
                value={content}
                onValueChange={(code) => setContent(code)}
                highlight={(code) =>
                  Prism.highlight(
                    code,
                    Prism.languages[category.toLowerCase()] || Prism.languages.javascript,
                    category.toLowerCase()
                  )
                }
                padding={16}
                style={{
                  backgroundColor: "#1e1e1e",
                  color: "white",
                  fontFamily: "monospace",
                  fontSize: 15,
                  borderRadius: 10,
                  minHeight: "300px",
                  outline: "none",
                }}
              />
              <button
                className="exit-code-mode"
                    onClick={() => {
                        setIsCodeMode(false);
                        const normalizedCategory = category.trim().toLowerCase();
                        if (normalizedCategory === "html/css") {
                            setContent((p) => `\n\`\`\`markup\n${p}\n\`\`\`\n`);
                            console.log("Текущее значение category:", category);
                        } else {
                            setContent((p) => `\n\`\`\`${normalizedCategory}\n${p}\n\`\`\`\n`);
                            console.log("Условие не выполнилось. Category:", category);
                        }
                    }}
              >
                Exit Code Mode
              </button>
            </div>
          ) : (
            <textarea
              id="topic-textarea"
              value={content}
              onChange={(e) => setContent(e.target.value)}
              className="topic-textarea"
              placeholder="Enter thread content..."
              disabled={loading}
            />
          )}
        </>
      ) : (
        <div className="topic-preview">
          <ReactMarkdown
            children={content}
            components={{
              code({ className, children }) {
                const match = /language-(\w+)/.exec(className || "");
                return match ? (
                  <SyntaxHighlighter
                    style={materialOceanic}
                    language={match[1]}
                    PreTag="div"
                    wrapLongLines={true}
                    customStyle={{
                      borderRadius: "10px",
                      background: "#1e1e1e",
                      padding: "16px",
                      fontSize: "14px",
                      lineHeight: "1.5",
                    }}
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

      <button onClick={handlePublish} className="publish-button" disabled={loading}>
        {loading ? "Publishing..." : "Publish"}
      </button>
    </div>
  );
};
