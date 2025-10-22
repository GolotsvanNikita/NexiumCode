import axios from 'axios';

const API_BASE_URL = 'http://localhost:5064/api';

export interface Thread
{
    id: number;
    userId: number;
    title: string;
    content: string;
    category: string;
    isResolved: boolean;
    createdAt: string;
    username: string;
    avatarUrl: string;
    replyCount: number;
    replies?: Reply[];
}


export interface Reply
{
    id: number;
    threadId: number;
    userId: number;
    content: string;
    parentReplyId: number | null;
    createdAt: string;
    username: string;
    avatarUrl: string;
    childReplies?: Reply[];
}

export interface CreateThreadRequest
{
    userId: number;
    title: string;
    content: string;
    category: string;
}

export interface CreateReplyRequest
{
    userId: number;
    content: string;
    parentReplyId?: number | null;
}

class ForumService
{
    async getThreads(category?: string, search?: string): Promise<Thread[]>
    {
        const params: any = {};
        if (category) params.category = category;
        if (search) params.search = search;

        const response = await axios.get(`${API_BASE_URL}/forum/threads`, { params });
        return response.data;
    }

    async getThread(threadId: number): Promise<Thread>
    {
        const response = await axios.get(`${API_BASE_URL}/forum/threads/${threadId}`);
        return response.data;
    }

    async createThread(data: CreateThreadRequest): Promise<{ threadId: number; message: string }>
    {
        const response = await axios.post(`${API_BASE_URL}/forum/threads`, data);
        return response.data;
    }

    async createReply(threadId: number, data: CreateReplyRequest): Promise<{ replyId: number; message: string }>
    {
        const response = await axios.post(`${API_BASE_URL}/forum/threads/${threadId}/replies`, data);
        return response.data;
    }

    async deleteThread(threadId: number, userId: number): Promise<{ message: string }>
    {
        const response = await axios.delete(`${API_BASE_URL}/forum/threads/${threadId}`, {
            params: { userId }
        });
        return response.data;
    }

    async markAsResolved(threadId: number, userId: number): Promise<{ message: string }>
    {
        const response = await axios.put(`${API_BASE_URL}/forum/threads/${threadId}/resolve`, null,
        {
            params: { userId }
        });
        return response.data;
    }

    async getCategories(): Promise<string[]>
    {
        const response = await axios.get(`${API_BASE_URL}/forum/categories`);
        return response.data;
    }
}

export const forumService = new ForumService();