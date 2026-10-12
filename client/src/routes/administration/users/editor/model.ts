import { createModel } from "cx/ui";

import type { Option } from "../../../../api/assets";
import type { UserDetail, UserForm } from "../../../../api/users";
import type { RecordState } from "../../../../recordController";

export interface UserDraft {
    email?: string | null;
    name?: string | null;
    roleId?: string | null;
    roleText?: string;
}

/** What the server says the reader may do to the user, as loaded. */
export interface UserFlags {
    isActive: boolean;
    isSelf: boolean;
    canManage: boolean;
}

export interface UserState extends RecordState<UserDraft> {
    options: { roles: Option[] };
    /** Absent while adding. */
    user?: UserFlags;
}

export interface Model {
    record: UserState;
    $route: { id: string };
}

export default createModel<Model>();

export const toDraft = (u: UserDetail): UserDraft => ({
    email: u.email,
    name: u.name,
    roleId: u.roleId,
    roleText: u.role,
});

export const toForm = (d: UserDraft): UserForm => ({
    email: (d.email ?? "").trim(),
    name: (d.name ?? "").trim(),
    roleId: d.roleId ?? null,
});
