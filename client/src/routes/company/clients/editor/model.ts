import { createModel } from "cx/ui";

import type { ClientDetail } from "../../../../api/clients";
import type { HoldingSection } from "../../../../holdings";

/** The form, as the fields bind it: text keys absent until typed. */
export interface ClientDraft {
    name?: string | null;
}

export interface ClientEditorState {
    /** `null` while creating. */
    id: string | null;
    viewing: boolean;
    title: string;
    draft: ClientDraft;
    /** Its projects, as the one section a client has. */
    sections: HoldingSection[];
    projectCount: number;
    loaded: boolean;
    loading: boolean;
    saving: boolean;
    error?: string;
    errors: { name?: string };
    valid: boolean;
    visited: boolean;
}

export interface Model {
    client: ClientEditorState;
    $route: { id: string };
}

export default createModel<Model>();

export const toForm = (draft: ClientDraft) => ({ name: (draft.name ?? "").trim() });

export const projectsText = (n: number) => (n === 1 ? "1 project" : `${n} projects`);

/** Its projects as a section, each linking to its page — ahead of the projects screen. */
export const toSections = (c: ClientDetail): HoldingSection[] =>
    c.projectCount === 0
        ? []
        : [
              {
                  key: "projects",
                  title: "Projects",
                  count: String(c.projectCount),
                  rows: c.projects.map((p) => ({
                      key: p.id,
                      title: p.name,
                      meta: p.owner ? `Led by ${p.owner}` : undefined,
                      href: `~/company/projects/${p.id}`,
                  })),
                  moreHref:
                      c.projectCount > c.projects.length ? `~/company/projects?clientId=${c.id}` : undefined,
                  moreText: c.projectCount > c.projects.length ? `See all ${c.projectCount}` : undefined,
              },
          ];
