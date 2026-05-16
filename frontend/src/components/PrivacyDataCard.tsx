import { useState } from "react";
import { useMutation } from "@tanstack/react-query";
import { useNavigate } from "@tanstack/react-router";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Alert, AlertDescription } from "@/components/ui/alert";
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
  DialogTrigger,
} from "@/components/ui/dialog";
import { useAuth } from "@/context/AuthContext";
import { apiFetch } from "@/lib/api";

export function PrivacyDataCard() {
  const { user, logout } = useAuth();
  const navigate = useNavigate();
  const [confirmText, setConfirmText] = useState("");
  const [dialogOpen, setDialogOpen] = useState(false);

  const downloadMutation = useMutation({
    mutationFn: async () => {
      const res = await apiFetch("/api/users/export", { method: "GET" });
      if (!res.ok) {
        throw new Error("Failed to export data");
      }

      // Try to read filename from Content-Disposition, fall back to a default
      const disposition = res.headers.get("Content-Disposition") ?? "";
      const match = disposition.match(/filename="?([^"]+)"?/);
      const filename = match?.[1] ?? "trivia-data-export.json";

      const blob = await res.blob();
      const url = URL.createObjectURL(blob);
      const link = document.createElement("a");
      link.href = url;
      link.download = filename;
      document.body.appendChild(link);
      link.click();
      link.remove();
      URL.revokeObjectURL(url);
    },
  });

  const deleteMutation = useMutation({
    mutationFn: async () => {
      const res = await apiFetch("/api/users/me", { method: "DELETE" });
      if (!res.ok) {
        throw new Error("Failed to delete account");
      }
    },
    onSuccess: async () => {
      await logout();
      navigate({ to: "/login" });
    },
  });

  const canDelete = user != null && confirmText === user.username;

  return (
    <Card>
      <CardHeader>
        <CardTitle>Data and Privacy</CardTitle>
      </CardHeader>
      <CardContent className="space-y-6">
        <section className="space-y-2">
          <h3 className="font-semibold">Download your data</h3>
          <p className="text-sm text-muted-foreground">
            Get a JSON file containing your account info, stats, and all quizzes you've
            created.
          </p>
          <Button
            variant="outline"
            onClick={() => downloadMutation.mutate()}
            disabled={downloadMutation.isPending}
          >
            {downloadMutation.isPending ? "Preparing…" : "Download my data"}
          </Button>
          {downloadMutation.isError && (
            <Alert variant="destructive">
              <AlertDescription>
                Could not export your data. Try again.
              </AlertDescription>
            </Alert>
          )}
        </section>

        <section className="space-y-2 border-t pt-6">
          <h3 className="font-semibold text-destructive">Delete your account</h3>
          <p className="text-sm text-muted-foreground">
            Permanently deletes your account, stats, and all quizzes you created. This
            cannot be undone.
          </p>

          <Dialog open={dialogOpen} onOpenChange={setDialogOpen}>
            <DialogTrigger asChild>
              <Button variant="destructive">Delete my account</Button>
            </DialogTrigger>
            <DialogContent>
              <DialogHeader>
                <DialogTitle>Are you absolutely sure?</DialogTitle>
                <DialogDescription>
                  This permanently deletes your account and everything tied to it. To
                  confirm, type your username{" "}
                  <span className="font-mono font-semibold">{user?.username}</span>{" "}
                  below.
                </DialogDescription>
              </DialogHeader>

              <div className="space-y-2">
                <Label htmlFor="confirm-username">Username</Label>
                <Input
                  id="confirm-username"
                  value={confirmText}
                  onChange={(e) => setConfirmText(e.target.value)}
                  autoComplete="off"
                />
              </div>

              {deleteMutation.isError && (
                <Alert variant="destructive">
                  <AlertDescription>
                    Could not delete your account. Try again.
                  </AlertDescription>
                </Alert>
              )}

              <DialogFooter>
                <Button variant="outline" onClick={() => setDialogOpen(false)}>
                  Cancel
                </Button>
                <Button
                  variant="destructive"
                  disabled={!canDelete || deleteMutation.isPending}
                  onClick={() => deleteMutation.mutate()}
                >
                  {deleteMutation.isPending ? "Deleting…" : "Delete forever"}
                </Button>
              </DialogFooter>
            </DialogContent>
          </Dialog>
        </section>
      </CardContent>
    </Card>
  );
}