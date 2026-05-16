import { useEffect, useMemo, useState } from "react";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { createFileRoute } from "@tanstack/react-router";
import { ShieldAlert } from "lucide-react";
import { Alert, AlertDescription, AlertTitle } from "@/components/ui/alert";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import {
  Card,
  CardContent,
  CardDescription,
  CardHeader,
  CardTitle,
} from "@/components/ui/card";
import { Input } from "@/components/ui/input";
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from "@/components/ui/table";
import { Textarea } from "@/components/ui/textarea";
import {
  banModerationUser,
  getModerationQuizDetails,
  getModerationUserQuizzes,
  searchModerationUsers,
  warnModerationUser,
  type ModerationQuiz,
  type ModerationUser,
} from "@/lib/moderation";

export const Route = createFileRoute("/_app/admin/")({
  component: AdminPage,
});

function AdminPage() {
  const queryClient = useQueryClient();
  const [search, setSearch] = useState("");
  const [selectedUser, setSelectedUser] = useState<ModerationUser | null>(null);
  const [selectedQuizId, setSelectedQuizId] = useState<number | null>(null);
  const [warningMessage, setWarningMessage] = useState("");
  const [banReason, setBanReason] = useState("");
  const [actionMessage, setActionMessage] = useState<string | null>(null);

  const debouncedSearch = useDebouncedValue(search, 300);

  const usersQuery = useQuery({
    queryKey: ["moderation", "users", debouncedSearch],
    queryFn: () => searchModerationUsers(debouncedSearch),
  });

  const quizzesQuery = useQuery({
    queryKey: ["moderation", "users", selectedUser?.id, "quizzes"],
    queryFn: () => getModerationUserQuizzes(selectedUser!.id),
    enabled: selectedUser !== null,
  });

  const quizDetailsQuery = useQuery({
    queryKey: ["moderation", "quizzes", selectedQuizId],
    queryFn: () => getModerationQuizDetails(selectedQuizId!),
    enabled: selectedQuizId !== null,
  });

  const warnMutation = useMutation({
    mutationFn: () => warnModerationUser(selectedUser!.id, warningMessage.trim()),
    onSuccess: (data) => {
      setActionMessage(data.message);
      setWarningMessage("");
    },
  });

  const banMutation = useMutation({
    mutationFn: () => banModerationUser(selectedUser!.id, banReason.trim()),
    onSuccess: (data) => {
      setActionMessage(data.message);
      setBanReason("");
      queryClient.invalidateQueries({ queryKey: ["moderation", "users"] });
    },
  });

  const authorizationError = useMemo(() => {
    const error = usersQuery.error;
    if (error instanceof Error && /401|403|Unauthorized|Forbidden/i.test(error.message)) {
      return error.message;
    }
    return null;
  }, [usersQuery.error]);

  const handleSelectUser = (user: ModerationUser) => {
    setSelectedUser(user);
    setSelectedQuizId(null);
    setActionMessage(null);
  };

  const handleSelectQuiz = (quiz: ModerationQuiz) => {
    setSelectedQuizId(quiz.id);
  };

  return (
    <div className="mx-auto flex w-full max-w-7xl flex-col gap-6">
      <div>
        <h1 className="text-4xl font-bold tracking-tight">Admin moderation</h1>
        <p className="mt-2 text-muted-foreground">
          Search users, inspect their quizzes, and send moderation actions.
        </p>
      </div>

      {authorizationError && (
        <Alert variant="destructive">
          <ShieldAlert />
          <AlertTitle>Access denied</AlertTitle>
          <AlertDescription>{authorizationError}</AlertDescription>
        </Alert>
      )}

      {actionMessage && (
        <Alert>
          <AlertTitle>Success</AlertTitle>
          <AlertDescription>{actionMessage}</AlertDescription>
        </Alert>
      )}

      <Card>
        <CardHeader>
          <CardTitle>User search</CardTitle>
          <CardDescription>Search by username or email.</CardDescription>
        </CardHeader>
        <CardContent>
          <Input
            value={search}
            onChange={(event) => setSearch(event.target.value)}
            placeholder="Search users..."
          />
        </CardContent>
      </Card>

      <Card>
        <CardHeader>
          <CardTitle>Users</CardTitle>
          <CardDescription>
            {usersQuery.isLoading ? "Loading users..." : `${usersQuery.data?.length ?? 0} users found`}
          </CardDescription>
        </CardHeader>
        <CardContent>
          {usersQuery.isError && !authorizationError ? (
            <ErrorAlert error={usersQuery.error} />
          ) : (
            <Table>
              <TableHeader>
                <TableRow>
                  <TableHead>Username</TableHead>
                  <TableHead>Email</TableHead>
                  <TableHead>Role</TableHead>
                  <TableHead>isBanned</TableHead>
                  <TableHead>banReason</TableHead>
                  <TableHead>bannedAt</TableHead>
                </TableRow>
              </TableHeader>
              <TableBody>
                {(usersQuery.data ?? []).map((user) => (
                  <TableRow
                    key={user.id}
                    onClick={() => handleSelectUser(user)}
                    data-state={selectedUser?.id === user.id ? "selected" : undefined}
                    className="cursor-pointer"
                  >
                    <TableCell className="font-medium">{user.username}</TableCell>
                    <TableCell>{user.email}</TableCell>
                    <TableCell>
                      <Badge variant={user.role === "Admin" ? "default" : "secondary"}>{user.role}</Badge>
                    </TableCell>
                    <TableCell>{user.isBanned ? "true" : "false"}</TableCell>
                    <TableCell>{user.banReason ?? "—"}</TableCell>
                    <TableCell>{formatDate(user.bannedAt)}</TableCell>
                  </TableRow>
                ))}
                {!usersQuery.isLoading && usersQuery.data?.length === 0 && (
                  <TableRow>
                    <TableCell colSpan={6} className="text-center text-muted-foreground">
                      No users found.
                    </TableCell>
                  </TableRow>
                )}
              </TableBody>
            </Table>
          )}
        </CardContent>
      </Card>

      <div className="grid gap-6 lg:grid-cols-[minmax(0,1fr)_minmax(0,1.4fr)]">
        <Card>
          <CardHeader>
            <CardTitle>Selected user's quizzes</CardTitle>
            <CardDescription>
              {selectedUser ? `Quizzes created by ${selectedUser.username}` : "Select a user to load quizzes."}
            </CardDescription>
          </CardHeader>
          <CardContent>
            {!selectedUser ? (
              <p className="text-sm text-muted-foreground">No user selected.</p>
            ) : quizzesQuery.isError ? (
              <ErrorAlert error={quizzesQuery.error} />
            ) : quizzesQuery.isLoading ? (
              <p className="text-sm text-muted-foreground">Loading quizzes...</p>
            ) : (quizzesQuery.data ?? []).length === 0 ? (
              <p className="text-sm text-muted-foreground">This user has no quizzes.</p>
            ) : (
              <div className="space-y-3">
                {quizzesQuery.data?.map((quiz) => (
                  <button
                    key={quiz.id}
                    type="button"
                    onClick={() => handleSelectQuiz(quiz)}
                    className={`w-full rounded-lg border p-4 text-left transition hover:bg-accent ${
                      selectedQuizId === quiz.id ? "bg-accent" : ""
                    }`}
                  >
                    <div className="font-medium">{quiz.title || "Untitled quiz"}</div>
                    <p className="mt-1 line-clamp-2 text-sm text-muted-foreground">
                      {quiz.description || "No description"}
                    </p>
                    <div className="mt-2 flex flex-wrap gap-2 text-xs text-muted-foreground">
                      <Badge variant="outline">{quiz.questionCount} questions</Badge>
                      <Badge variant="secondary">{quiz.timesPlayed} plays</Badge>
                    </div>
                  </button>
                ))}
              </div>
            )}
          </CardContent>
        </Card>

        <Card>
          <CardHeader>
            <CardTitle>Quiz details</CardTitle>
            <CardDescription>
              {selectedQuizId ? "Questions, options, and correct answer index." : "Select a quiz to inspect it."}
            </CardDescription>
          </CardHeader>
          <CardContent>
            {!selectedQuizId ? (
              <p className="text-sm text-muted-foreground">No quiz selected.</p>
            ) : quizDetailsQuery.isError ? (
              <ErrorAlert error={quizDetailsQuery.error} />
            ) : quizDetailsQuery.isLoading ? (
              <p className="text-sm text-muted-foreground">Loading quiz details...</p>
            ) : quizDetailsQuery.data ? (
              <div className="space-y-5">
                <div>
                  <h2 className="text-xl font-semibold">{quizDetailsQuery.data.title || "Untitled quiz"}</h2>
                  <p className="text-sm text-muted-foreground">
                    By {quizDetailsQuery.data.creatorUsername} · {quizDetailsQuery.data.timesPlayed} plays
                  </p>
                  <p className="mt-2 text-sm">{quizDetailsQuery.data.description || "No description"}</p>
                </div>
                <div className="space-y-4">
                  {quizDetailsQuery.data.questions.map((question, questionIndex) => (
                    <div key={question.id} className="rounded-lg border p-4">
                      <div className="flex flex-wrap items-start justify-between gap-2">
                        <h3 className="font-medium">
                          {questionIndex + 1}. {question.questionText}
                        </h3>
                        <Badge variant="outline">Correct index: {question.correctOptionIndex}</Badge>
                      </div>
                      <p className="mt-1 text-xs text-muted-foreground">Time limit: {question.timeLimit}s</p>
                      <ol className="mt-3 list-decimal space-y-2 pl-5 text-sm">
                        {question.options.map((option, optionIndex) => (
                          <li key={option.id}>
                            <span className={optionIndex === question.correctOptionIndex ? "font-semibold" : undefined}>
                              {option.optionText}
                            </span>
                          </li>
                        ))}
                      </ol>
                    </div>
                  ))}
                </div>
              </div>
            ) : null}
          </CardContent>
        </Card>
      </div>

      <div className="grid gap-6 lg:grid-cols-2">
        <Card>
          <CardHeader>
            <CardTitle>Warn user</CardTitle>
            <CardDescription>Send a warning to the selected user.</CardDescription>
          </CardHeader>
          <CardContent>
            <form
              className="space-y-3"
              onSubmit={(event) => {
                event.preventDefault();
                setActionMessage(null);
                if (selectedUser && warningMessage.trim()) {
                  warnMutation.mutate();
                }
              }}
            >
              <Textarea
                value={warningMessage}
                onChange={(event) => setWarningMessage(event.target.value)}
                placeholder="Warning message"
              />
              {warnMutation.isError && <ErrorAlert error={warnMutation.error} />}
              <Button type="submit" disabled={!selectedUser || !warningMessage.trim() || warnMutation.isPending}>
                {warnMutation.isPending ? "Sending..." : "Send warning"}
              </Button>
            </form>
          </CardContent>
        </Card>

        <Card>
          <CardHeader>
            <CardTitle>Ban user</CardTitle>
            <CardDescription>Ban the selected user with a reason.</CardDescription>
          </CardHeader>
          <CardContent>
            <form
              className="space-y-3"
              onSubmit={(event) => {
                event.preventDefault();
                setActionMessage(null);
                if (selectedUser && banReason.trim()) {
                  banMutation.mutate();
                }
              }}
            >
              <Textarea
                value={banReason}
                onChange={(event) => setBanReason(event.target.value)}
                placeholder="Ban reason"
              />
              {banMutation.isError && <ErrorAlert error={banMutation.error} />}
              <Button
                type="submit"
                variant="destructive"
                disabled={!selectedUser || !banReason.trim() || banMutation.isPending}
              >
                {banMutation.isPending ? "Banning..." : "Ban user"}
              </Button>
            </form>
          </CardContent>
        </Card>
      </div>
    </div>
  );
}

function ErrorAlert({ error }: { error: unknown }) {
  return (
    <Alert variant="destructive">
      <AlertTitle>Error</AlertTitle>
      <AlertDescription>{error instanceof Error ? error.message : "Something went wrong."}</AlertDescription>
    </Alert>
  );
}

function formatDate(value: string | null) {
  if (!value) {
    return "—";
  }

  return new Intl.DateTimeFormat(undefined, {
    dateStyle: "medium",
    timeStyle: "short",
  }).format(new Date(value));
}

function useDebouncedValue<T>(value: T, delay: number) {
  const [debouncedValue, setDebouncedValue] = useState(value);

  useEffect(() => {
    const timeout = window.setTimeout(() => setDebouncedValue(value), delay);
    return () => window.clearTimeout(timeout);
  }, [value, delay]);

  return debouncedValue;
}
