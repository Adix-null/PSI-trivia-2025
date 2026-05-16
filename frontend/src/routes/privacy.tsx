import { createFileRoute } from "@tanstack/react-router";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";

export const Route = createFileRoute("/privacy")({
  component: PrivacyPage,
});

function PrivacyPage() {
  return (
    <div className="container mx-auto max-w-3xl px-4 py-8">
      <Card>
        <CardHeader>
          <CardTitle className="text-2xl">Privacy Policy</CardTitle>
        </CardHeader>
        <CardContent className="space-y-6 text-sm leading-relaxed">
          <section>
            <h2 className="font-semibold text-base mb-2">Who we are</h2>
            <p>
              PSI Trivia is a student project built for the Programų sistemų inžinerija
              course at Vilnius University. This site is operated by the student team
              for educational purposes.
            </p>
          </section>

          <section>
            <h2 className="font-semibold text-base mb-2">What data we collect</h2>
            <p>When you create an account, we store:</p>
            <ul className="list-disc pl-6 mt-2 space-y-1">
              <li>Your username and email address</li>
              <li>A bcrypt hash of your password (never the password itself)</li>
              <li>Your gameplay statistics (games played, won, quizzes created)</li>
              <li>Any quizzes and questions you create</li>
              <li>A JWT authentication token stored in an HTTP-only cookie</li>
            </ul>
            <p className="mt-2">
              Guest players who join games without registering are not stored in our
              database.
            </p>
          </section>

          <section>
            <h2 className="font-semibold text-base mb-2">Why we store it</h2>
            <p>
              We use this data to let you log in, save your quizzes, display your stats
              on leaderboards, and let other players see who created a quiz.
            </p>
          </section>

          <section>
            <h2 className="font-semibold text-base mb-2">Who can see your data</h2>
            <p>
              Your username and stats are visible on public leaderboards and profile
              pages. Your email address is private and only visible to you. Other users
              cannot see it.
            </p>
          </section>

          <section>
            <h2 className="font-semibold text-base mb-2">Your rights under GDPR</h2>
            <p>You can, at any time:</p>
            <ul className="list-disc pl-6 mt-2 space-y-1">
              <li>
                <strong>Download your data</strong> as a JSON file from your profile page
                (right to data portability, Article 20)
              </li>
              <li>
                <strong>Delete your account</strong> and all associated data from your
                profile page (right to erasure, Article 17)
              </li>
              <li>Update your username, email, or password from your profile</li>
            </ul>
          </section>

          <section>
            <h2 className="font-semibold text-base mb-2">Third parties</h2>
            <p>
              The backend is hosted on Render, the frontend on Netlify, and the database
              on Supabase. These providers process data on our behalf to keep the site
              running.
            </p>
          </section>

          <section>
            <h2 className="font-semibold text-base mb-2">Changes to this policy</h2>
            <p>
              As a student project, this policy may change as the project evolves. The
              current version is always available at this page.
            </p>
          </section>
        </CardContent>
      </Card>
    </div>
  );
}