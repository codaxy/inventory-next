import type { Page } from "../paging";
import type { Option, Ref, Section } from "./assets";
import { send, toQuery } from "./http";

export interface ProjectItem {
    id: string;
    name: string;
    client: string;
    owner: string;
    information: number;
}

export interface ProjectDetail {
    id: string;
    name: string;
    client: Ref;
    owner: Ref;
    information: Section<{ id: string; name: string; type: string | null }>;
}

export interface ProjectForm {
    name: string;
    clientId: string | null;
    ownerId: string | null;
}

export type ProjectSort = `${"" | "-"}${"name" | "client" | "owner"}`;

export interface ProjectQuery {
    q?: string;
    clientId?: string;
    /** The owner. */
    personId?: string;
    sort?: ProjectSort;
    page: number;
    pageSize: number;
}

const base = "/api/company/projects";

export const listProjects = (q: ProjectQuery) => send<Page<ProjectItem>>(`${base}/?${toQuery(q)}`);
export const getProject = (id: string) => send<ProjectDetail>(`${base}/${id}`);
export const getProjectOptions = () => send<{ clients: Option[]; people: Option[] }>(`${base}/options`);
export const createProject = (form: ProjectForm) =>
    send<ProjectDetail>(`${base}/`, { method: "POST", body: JSON.stringify(form) });
export const updateProject = (id: string, form: ProjectForm) =>
    send<ProjectDetail>(`${base}/${id}`, { method: "PUT", body: JSON.stringify(form) });
export const deleteProject = (id: string) => send<void>(`${base}/${id}`, { method: "DELETE" });
