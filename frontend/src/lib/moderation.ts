import { apiFetch } from "@/lib/api";

export interface ModerationUser {
  id: number;
  username: string;
  email: string;
  role: string;
  isBanned: boolean;
  banReason: string | null;
  bannedAt: string | null;
}

export interface ModerationQuiz {
  id: number;
  title: string;
  description: string;
  timesPlayed: number;
  questionCount: number;
}

export interface ModerationQuizDetails {
  id: number;
  creatorId: number;
  creatorUsername: string;
  title: string;
  description: string;
  timesPlayed: number;
  questions: ModerationQuestion[];
}

export interface ModerationQuestion {
  id: number;
  questionText: string;
  correctOptionIndex: number;
  timeLimit: number;
  options: ModerationOption[];
}

export interface ModerationOption {
  id: number;
  optionText: string;
}

export interface ModerationMessageResponse {
  message: string;
}

async function parseErrorResponse(response: Response) {
  const data = await response.json().catch(() => null);
  const message =
    data && typeof data === "object" && "message" in data && typeof data.message === "string"
      ? data.message
      : `Request failed with status ${response.status}`;

  return new Error(message);
}

async function fetchJson<T>(path: string, init?: RequestInit): Promise<T> {
  const response = await apiFetch(path, init);

  if (!response.ok) {
    throw await parseErrorResponse(response);
  }

  return response.json() as Promise<T>;
}

export function searchModerationUsers(search: string) {
  const params = new URLSearchParams();
  if (search.trim()) {
    params.set("search", search.trim());
  }

  const query = params.toString();
  return fetchJson<ModerationUser[]>(`/api/moderation/users${query ? `?${query}` : ""}`);
}

export function getModerationUserQuizzes(userId: number) {
  return fetchJson<ModerationQuiz[]>(`/api/moderation/users/${userId}/quizzes`);
}

export function getModerationQuizDetails(quizId: number) {
  return fetchJson<ModerationQuizDetails>(`/api/moderation/quizzes/${quizId}`);
}

export function warnModerationUser(userId: number, message: string) {
  return fetchJson<ModerationMessageResponse>("/api/moderation/warn", {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify({ userId, message }),
  });
}

export function banModerationUser(userId: number, reason: string) {
  return fetchJson<ModerationMessageResponse>("/api/moderation/ban", {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify({ userId, reason }),
  });
}