// Files scenario of the Leonardo web sample.
// Plain JavaScript, no framework. Hash routing gives every view its own URL:
//   #/files                                   Customers (all customer folders)
//   #/files/hofmann-logistics/contracts       a folder
//   #/files/hofmann-logistics/contracts?file=price-list-2027-xlsx   a folder with a file's details
//   #/settings                                Settings
// All content is invented. Nothing leaves the browser; "uploaded" files exist in this page only.

"use strict";

(() => {
    // ---------- Sample data (matches the WinUI gallery, samples/UpInBlue.Leonardo.Gallery/Models/SampleData.cs) ----------

    const signedInUser = "Anna Berg";

    const file = (name, modified, modifiedBy, size, extra = {}) => ({
        name,
        kind: kindOf(name),
        modified: new Date(modified),
        modifiedBy,
        size,
        ...extra,
    });

    const folder = (name, { folders = [], files = [] } = {}) => ({ name, folders, files, expanded: false });

    const root = folder("Customers", {
        folders: [
            folder("Hofmann Logistics", {
                folders: [
                    folder("Contracts", {
                        files: [
                            file("Framework agreement 2026.docx", "2026-11-18T10:42:00+01:00", "Lea Schmid", 248_310),
                            file("Rahmenvertragsverlängerungsvereinbarung 2027.docx", "2026-11-19T16:05:00+01:00", "Anna Berg", 194_880),
                            file("Price list 2027.xlsx", "2026-11-17T09:12:00+01:00", "Jonas Weber", 88_412, {
                                status: { tone: "caution", text: "Checked out by Jonas Weber" },
                            }),
                            file("Signed framework agreement 2025.pdf", "2026-11-03T11:30:00+01:00", "Tim Kraus", 1_284_002),
                            file("Service level appendix.docx", "2026-11-12T14:18:00+01:00", "Lea Schmid", 61_740),
                        ],
                    }),
                    folder("Invoices", {
                        files: [
                            file("Invoice 2026-118.pdf", "2026-11-15T08:55:00+01:00", "Mara Lindqvist", 402_118),
                            file("Invoice 2026-119.pdf", "2026-11-20T08:40:00+01:00", "Mara Lindqvist", 398_204),
                        ],
                    }),
                ],
            }),
            folder("Brenner Industries", {
                folders: [
                    folder("Plant expansion", {
                        files: [
                            file("Site plan hall 4.pdf", "2026-11-14T13:20:00+01:00", "Felix Okafor", 5_402_331),
                            file("Cost estimate.xlsx", "2026-11-16T17:02:00+01:00", "Jonas Weber", 120_554),
                            file("Photo foundation.jpg", "2026-11-11T10:15:00+01:00", "Felix Okafor", 3_120_448),
                        ],
                    }),
                ],
            }),
            folder("Lakeside Utilities", {
                files: [file("Meeting notes November.docx", "2026-11-19T15:48:00+01:00", "Anna Berg", 42_116)],
            }),
            folder("Kessler Construction", { folders: [folder("Tenders")] }),
            folder("Mertens Medical", {
                files: [
                    file("Onboarding checklist.docx", "2026-11-09T09:00:00+01:00", "Tim Kraus", 36_002),
                    file("Data processing agreement.pdf", "2026-11-10T16:41:00+01:00", "Lea Schmid", 860_271),
                ],
            }),
        ],
    });

    // ---------- Formatting (en-US, like the gallery) ----------

    const timeZone = "Europe/Berlin";
    const dateFormat = new Intl.DateTimeFormat("en-US", { month: "short", day: "numeric", year: "numeric", timeZone });
    const dateTimeFormat = new Intl.DateTimeFormat("en-US", {
        month: "short", day: "numeric", year: "numeric", hour: "numeric", minute: "2-digit", timeZone,
    });
    const numberFormat = new Intl.NumberFormat("en-US", { maximumFractionDigits: 0 });
    const sizeFormat = new Intl.NumberFormat("en-US", { minimumFractionDigits: 1, maximumFractionDigits: 1 });

    function formatSize(bytes) {
        if (bytes < 1024) return `${numberFormat.format(bytes)} bytes`;
        if (bytes < 1024 * 1024) return `${numberFormat.format(bytes / 1024)} KB`;
        return `${sizeFormat.format(bytes / (1024 * 1024))} MB`;
    }

    function plural(count, one, many) {
        return `${numberFormat.format(count)} ${count === 1 ? one : many}`;
    }

    function kindOf(name) {
        const extension = name.includes(".") ? name.split(".").pop().toLowerCase() : "";
        return {
            docx: "Word document",
            xlsx: "Excel workbook",
            pptx: "PowerPoint presentation",
            pdf: "PDF document",
            jpg: "JPEG image",
            jpeg: "JPEG image",
            png: "PNG image",
            txt: "Text document",
        }[extension] ?? "File";
    }

    // ---------- Model helpers ----------

    function slugify(text) {
        const slug = text
            .normalize("NFKD")
            .replace(/[̀-ͯ]/g, "")
            .toLowerCase()
            .replace(/[^a-z0-9]+/g, "-")
            .replace(/^-+|-+$/g, "");
        return slug || "item";
    }

    function uniqueSlug(text, taken) {
        const base = slugify(text);
        let slug = base;
        for (let index = 2; taken.has(slug); index++) slug = `${base}-${index}`;
        return slug;
    }

    function prepare(node, parent) {
        node.parent = parent;
        node.slug = parent ? slugify(node.name) : "";
        node.path = parent ? [...parent.path, node.slug] : [];
        const taken = new Set();
        for (const item of node.files) {
            item.id = uniqueSlug(item.name, taken);
            taken.add(item.id);
        }
        for (const child of node.folders) prepare(child, node);
    }

    prepare(root, null);

    const folderUrl = (node) => (node.path.length ? `#/files/${node.path.join("/")}` : "#/files");
    const fileUrl = (node, item) => `${folderUrl(node)}?file=${encodeURIComponent(item.id)}`;

    function trailOf(node) {
        const trail = [];
        for (let current = node; current; current = current.parent) trail.unshift(current);
        return trail;
    }

    function latestModified(node) {
        let latest = null;
        for (const item of node.files) if (!latest || item.modified > latest) latest = item.modified;
        for (const child of node.folders) {
            const childLatest = latestModified(child);
            if (childLatest && (!latest || childLatest > latest)) latest = childLatest;
        }
        return latest;
    }

    // ---------- Elements ----------

    const $ = (selector) => document.querySelector(selector);
    const html = document.documentElement;
    const el = {
        skipLink: $("[data-skip-link]"),
        appLinks: [...document.querySelectorAll("[data-section]")],
        folders: $("[data-folders]"),
        breadcrumb: $("[data-breadcrumb]"),
        breadcrumbList: $("[data-breadcrumb-list]"),
        placeName: $("[data-place-name]"),
        summary: $("[data-summary]"),
        commands: $("[data-commands]"),
        feedback: $("[data-feedback]"),
        feedbackText: $("[data-feedback-text]"),
        undo: $("[data-undo]"),
        content: $("[data-files-content]"),
        settings: $("[data-settings]"),
        missing: $("[data-missing]"),
        missingText: $("[data-missing-text]"),
        details: $("[data-details]"),
        fileInput: $("[data-file-input]"),
        announcer: $("[data-announcer]"),
    };

    function create(tag, properties = {}, ...children) {
        const node = document.createElement(tag);
        for (const [key, value] of Object.entries(properties)) {
            if (value === undefined || value === null || value === false) continue;
            if (key === "className") node.className = value;
            else if (key === "text") node.textContent = value;
            else if (key === "dataset") Object.assign(node.dataset, value);
            else if (key.startsWith("on")) node.addEventListener(key.slice(2), value);
            else node.setAttribute(key, value === true ? "" : value);
        }
        node.append(...children.filter((child) => child !== null && child !== undefined && child !== false));
        return node;
    }

    // ---------- State ----------

    let current = { view: null, folder: null, fileId: null };
    let feedback = null; // { message, undo }
    let pendingFocus = null; // a function that returns the element to focus after the next render

    // ---------- Routing ----------

    function parseRoute() {
        const raw = location.hash.replace(/^#/, "");
        if (raw === "" || raw === "/") return { redirect: "#/files" };

        const [path, query = ""] = raw.split("?");
        let segments;
        try {
            segments = path.split("/").filter(Boolean).map(decodeURIComponent);
        } catch {
            return { view: "missing", what: "page" };
        }

        if (segments.length === 1 && segments[0] === "settings") return { view: "settings" };

        if (segments[0] === "files") {
            let node = root;
            for (const segment of segments.slice(1)) {
                node = node.folders.find((child) => child.slug === segment);
                if (!node) return { view: "missing", what: "folder" };
            }
            return { view: "files", folder: node, fileId: new URLSearchParams(query).get("file") };
        }

        return { view: "missing", what: "page" };
    }

    function go(url, { replace = false } = {}) {
        if (location.hash === url) {
            render();
        } else if (replace) {
            location.replace(url);
        } else {
            location.hash = url;
        }
    }

    // ---------- Rendering ----------

    function render() {
        const route = parseRoute();
        if (route.redirect) {
            location.replace(route.redirect);
            return;
        }

        const previous = current;
        const focusKey = document.activeElement?.dataset?.focusKey;

        html.dataset.view = route.view;
        renderAppNav(route);

        el.settings.hidden = route.view !== "settings";
        el.missing.hidden = route.view !== "missing";
        el.content.hidden = route.view !== "files";
        // Customers lists customer folders only; files are uploaded into a customer's folder.
        el.commands.hidden = route.view !== "files" || route.folder === root;
        el.breadcrumb.hidden = route.view !== "files" || route.folder === root;

        let folderChanged = false;
        if (route.view === "files") {
            folderChanged = previous.view !== "files" || previous.folder !== route.folder;
            if (folderChanged) {
                feedback = null;
                for (const item of route.folder.files) item.isNew = false;
            }
            renderPlace(route.folder);
            renderTable(route.folder, route.fileId);
            renderDetails(route.folder, route.fileId);
            renderFolderNav(route.folder);
            document.title = `${route.folder.name} – Files – Leonardo web sample`;
        } else if (route.view === "settings") {
            feedback = null;
            el.placeName.textContent = "Settings";
            el.summary.textContent = "";
            document.title = "Settings – Leonardo web sample";
        } else {
            feedback = null;
            const what = route.what === "folder" ? "Folder" : "Page";
            el.placeName.textContent = `${what} not found`;
            el.summary.textContent = "";
            el.missingText.textContent = route.what === "folder"
                ? "This folder does not exist. It may have been renamed or moved."
                : "There is no page at this address.";
            document.title = `${what} not found – Leonardo web sample`;
        }
        renderFeedback();

        const viewChanged = previous.view !== route.view || folderChanged;
        if (viewChanged && previous.view !== null) animateEntering(route, previous);

        current = { view: route.view, folder: route.folder ?? null, fileId: route.fileId ?? null };
        restoreFocus(focusKey, route, viewChanged, previous);
    }

    function renderAppNav(route) {
        for (const link of el.appLinks) {
            const section = link.dataset.section;
            if (section === "settings" && route.view === "settings") link.setAttribute("aria-current", "page");
            else if (section === "files" && route.view === "files") {
                link.setAttribute("aria-current", route.folder === root && !route.fileId ? "page" : "true");
            } else link.removeAttribute("aria-current");
        }
    }

    function summaryOf(node) {
        const parts = [];
        if (node.folders.length) parts.push(plural(node.folders.length, "folder", "folders"));
        if (node.files.length) {
            const total = node.files.reduce((sum, item) => sum + item.size, 0);
            parts.push(`${plural(node.files.length, "file", "files")}, ${formatSize(total)}`);
        }
        return parts.length ? parts.join(", ") : "Empty folder";
    }

    function renderPlace(node) {
        el.breadcrumbList.replaceChildren(
            ...trailOf(node).map((step) => create("li", {},
                create("a", {
                    href: folderUrl(step),
                    "aria-current": step === node ? "page" : null,
                    dataset: { focusKey: `crumb:${step.path.join("/")}` },
                    text: step.name,
                }))),
        );
        el.placeName.textContent = node.name;
        el.summary.textContent = summaryOf(node);
    }

    function statusDot(status) {
        return create("span", { className: `status-dot status-${status.tone}`, "aria-hidden": "true" });
    }

    function renderTable(node, fileId) {
        if (!node.folders.length && !node.files.length) {
            el.content.replaceChildren(
                create("div", { className: "empty" },
                    create("h2", { className: "role-section", text: "This folder is empty" }),
                    create("p", { className: "role-body", text: "Files you upload to this folder appear here." }),
                    create("button", {
                        type: "button",
                        className: "button button-accent",
                        dataset: { action: "upload", focusKey: "empty-upload" },
                        text: "Upload files",
                    })),
            );
            return;
        }

        const header = create("tr", {},
            create("th", { scope: "col", className: "role-meta", text: "Name" }),
            create("th", { scope: "col", className: "role-meta col-kind", text: "Kind" }),
            create("th", { scope: "col", className: "role-meta col-modified", text: "Modified" }),
            create("th", { scope: "col", className: "role-meta col-by", text: "Modified by" }),
            create("th", { scope: "col", className: "role-meta col-size", text: "Size" }));

        const folderRows = node.folders.map((child) => {
            const latest = latestModified(child);
            const count = child.folders.length + child.files.length;
            return create("tr", {},
                create("td", {},
                    create("a", {
                        className: "row-link",
                        href: folderUrl(child),
                        dataset: { focusKey: `row:${child.path.join("/")}` },
                        text: child.name,
                    })),
                create("td", { className: "col-kind", text: "Folder" }),
                create("td", { className: "col-modified" },
                    latest ? create("time", { datetime: latest.toISOString(), text: dateFormat.format(latest) }) : null),
                create("td", { className: "col-by" }),
                create("td", { className: "col-size", text: count ? plural(count, "item", "items") : "Empty" }));
        });

        const fileRows = node.files.map((item) => {
            const isCurrent = item.id === fileId;
            const classes = [isCurrent ? "is-current" : "", item.isNew ? "is-new" : ""].filter(Boolean).join(" ");
            return create("tr", { className: classes || null, dataset: { fileId: item.id } },
                create("td", {},
                    item.status ? statusDot(item.status) : null,
                    create("a", {
                        className: "row-link",
                        href: fileUrl(node, item),
                        "aria-current": isCurrent ? "page" : null,
                        dataset: { focusKey: `file:${item.id}` },
                        text: item.name,
                    }),
                    item.status ? create("span", { className: "row-status role-meta status-text", text: item.status.text }) : null),
                create("td", { className: "col-kind", text: item.kind }),
                create("td", { className: "col-modified" },
                    create("time", { datetime: item.modified.toISOString(), text: dateFormat.format(item.modified) })),
                create("td", { className: "col-by", text: item.modifiedBy }),
                create("td", { className: "col-size", text: formatSize(item.size) }));
        });

        el.content.replaceChildren(
            create("table", { className: "file-table" },
                create("caption", { className: "visually-hidden", text: `Contents of ${node.name}` }),
                create("thead", {}, header),
                create("tbody", {}, ...folderRows, ...fileRows)),
        );
    }

    function renderDetails(node, fileId) {
        const item = fileId ? node.files.find((candidate) => candidate.id === fileId) : null;
        let body;

        if (!fileId) {
            const hint = node.files.length
                ? "Select a file to see its details."
                : node.folders.length
                    ? "Open a folder and select a file to see its details."
                    : "Details of the files in this folder appear here.";
            body = create("p", { className: "details-hint", text: hint });
        } else if (!item) {
            body = create("div", { className: "details-body" },
                create("h2", { className: "role-section", tabindex: "-1", dataset: { detailsHeading: "" }, text: "File not found" }),
                create("p", { className: "role-body", text: `This file is no longer in ${node.name}. It may have been deleted or moved.` }),
                create("p", { className: "role-body" },
                    create("a", { href: folderUrl(node), text: `Show all of ${node.name}` })));
        } else {
            const field = (label, ...value) => create("div", {}, create("dt", { text: label }), create("dd", {}, ...value));
            body = create("div", { className: "details-body" },
                create("div", {},
                    create("h2", { className: "role-section", tabindex: "-1", dataset: { detailsHeading: "" }, text: item.name }),
                    create("p", { className: "role-meta", text: item.kind })),
                item.status
                    ? create("p", { className: "details-status" }, statusDot(item.status), create("span", { text: item.status.text }))
                    : null,
                create("dl", {},
                    field("Modified", create("time", { datetime: item.modified.toISOString(), text: dateTimeFormat.format(item.modified) })),
                    field("Modified by", item.modifiedBy),
                    field("Size", formatSize(item.size)),
                    field("Location", trailOf(node).map((step) => step.name).join(" / "))),
                create("div", { className: "details-actions" },
                    create("button", {
                        type: "button",
                        className: "button",
                        dataset: { action: "delete", focusKey: "details-delete" },
                        text: "Delete file",
                    })));
        }

        el.details.replaceChildren(body);
    }

    // ---------- Folder navigation: APG disclosure navigation with nested lists of links ----------

    const chevron = () => {
        const svg = document.createElementNS("http://www.w3.org/2000/svg", "svg");
        svg.setAttribute("viewBox", "0 0 16 16");
        svg.setAttribute("aria-hidden", "true");
        svg.setAttribute("focusable", "false");
        const path = document.createElementNS("http://www.w3.org/2000/svg", "path");
        path.setAttribute("d", "M5.7 3.3a1 1 0 0 1 1.4 0l4 4a1 1 0 0 1 0 1.4l-4 4a1 1 0 0 1-1.4-1.4L9 8 5.7 4.7a1 1 0 0 1 0-1.4z");
        svg.append(path);
        return svg;
    };

    const navItems = new Map(); // folder -> { link, button, list }

    function buildFolderList(nodes) {
        return create("ul", {}, ...nodes.map((node) => {
            const id = `folders-${node.path.join("-")}`;
            const link = create("a", {
                className: "folder-link",
                href: folderUrl(node),
                dataset: { focusKey: `nav:${node.path.join("/")}` },
                text: node.name,
            });
            let button = null;
            let list = null;
            if (node.folders.length) {
                list = buildFolderList(node.folders);
                list.id = id;
                list.hidden = true;
                button = create("button", {
                    type: "button",
                    className: "disclosure",
                    "aria-expanded": "false",
                    "aria-controls": id,
                    onclick: () => setExpanded(node, !node.expanded),
                }, chevron(), create("span", { className: "visually-hidden", text: `Subfolders of ${node.name}` }));
            }
            navItems.set(node, { link, button, list });
            return create("li", {},
                create("div", { className: "folder-row" }, button ?? create("span", { className: "disclosure-spacer" }), link),
                list);
        }));
    }

    function setExpanded(node, expanded) {
        const item = navItems.get(node);
        if (!item?.button) return;
        node.expanded = expanded;
        item.button.setAttribute("aria-expanded", String(expanded));
        item.list.hidden = !expanded;
    }

    function renderFolderNav(selected) {
        for (const [node, item] of navItems) {
            if (node === selected) item.link.setAttribute("aria-current", "page");
            else item.link.removeAttribute("aria-current");
        }
        // The selected folder and its parents are open, so the selection is always visible.
        for (const step of trailOf(selected)) setExpanded(step, true);
    }

    el.folders.append(buildFolderList(root.folders));

    // ---------- Feedback: what just happened, with undo ----------

    function renderFeedback() {
        el.feedbackText.textContent = feedback?.message ?? "";
        el.feedback.classList.toggle("has-message", Boolean(feedback));
        el.undo.hidden = !feedback?.undo;
    }

    el.undo.addEventListener("click", () => {
        const undo = feedback?.undo;
        if (undo) undo();
    });

    // ---------- Motion ----------

    function depthOf(route) {
        if (route.view === "files") return route.folder.path.length + 1;
        return 0;
    }

    function animateEntering(route, previous) {
        const isBack = depthOf(route) < depthOf(previous);
        // Stagger: at most four groups, in reading order.
        const groups = [el.breadcrumb, el.placeName, el.summary, route.view === "settings" ? el.settings : route.view === "missing" ? el.missing : el.content];
        groups.forEach((group) => {
            group.classList.remove("is-entering", "is-back");
            group.style.removeProperty("--files-group");
        });
        // Restart the animation; an animation that is still running jumps to its end.
        void el.placeName.offsetWidth;
        groups.forEach((group, index) => {
            group.style.setProperty("--files-group", String(index));
            group.classList.add("is-entering");
            group.classList.toggle("is-back", isBack);
        });
    }

    function leave(row) {
        if (!row) return Promise.resolve();
        row.classList.add("is-leaving");
        const animations = row.getAnimations ? row.getAnimations() : [];
        return Promise.all(animations.map((animation) => animation.finished.catch(() => undefined)));
    }

    // ---------- Focus ----------

    function announce(message) {
        // Clear first so that the same message is announced again.
        el.announcer.textContent = "";
        requestAnimationFrame(() => { el.announcer.textContent = message; });
    }

    function isStacked() {
        // The details pane sits below the list in medium and narrow windows.
        return el.details.getBoundingClientRect().top >= el.content.getBoundingClientRect().bottom;
    }

    function restoreFocus(focusKey, route, viewChanged, previous) {
        if (pendingFocus) {
            const target = pendingFocus();
            pendingFocus = null;
            if (target) {
                target.focus();
                return;
            }
        }

        const active = document.activeElement;
        const focusLost = !active || active === document.body || !active.isConnected;

        // Same view (for example another file selected in the list): focus returns to the element the user used.
        if (focusLost && focusKey && !viewChanged) {
            const same = document.querySelector(`[data-focus-key="${CSS.escape(focusKey)}"]`);
            if (same) {
                same.focus();
                if (route.view === "files" && route.fileId && route.fileId !== previous.fileId) {
                    if (isStacked()) $("[data-details-heading]")?.focus();
                    else announce(`Details of ${$("[data-details-heading]")?.textContent ?? ""}`);
                }
                return;
            }
        }

        if (focusLost && viewChanged && previous.view !== null) {
            // Dive in: the new place name receives focus, so screen readers read where the user is now.
            el.placeName.focus();
            return;
        }

        if (!focusLost && viewChanged && previous.view !== null) {
            // Focus stayed in a navigation (folders or banner); announce the new place.
            announce(route.view === "files" ? `${route.folder.name}, ${summaryOf(route.folder)}` : el.placeName.textContent);
        }
    }

    // ---------- Commands ----------

    document.addEventListener("click", (event) => {
        const target = event.target instanceof Element ? event.target.closest("[data-action]") : null;
        if (!target) return;
        if (target.dataset.action === "upload") el.fileInput.click();
        if (target.dataset.action === "delete") deleteCurrentFile();
    });

    async function deleteCurrentFile() {
        const node = current.folder;
        const index = node?.files.findIndex((item) => item.id === current.fileId) ?? -1;
        if (index < 0) return;
        const item = node.files[index];

        await leave(el.content.querySelector(`tr[data-file-id="${CSS.escape(item.id)}"]`));
        node.files.splice(index, 1);

        feedback = {
            message: `Deleted “${item.name}”.`,
            undo: () => {
                node.files.splice(Math.min(index, node.files.length), 0, item);
                item.isNew = true;
                feedback = { message: `Restored “${item.name}”.` };
                pendingFocus = () => document.querySelector(`[data-focus-key="file:${CSS.escape(item.id)}"]`);
                go(fileUrl(node, item), { replace: true });
            },
        };
        pendingFocus = () => el.undo;
        go(folderUrl(node), { replace: true });
    }

    el.fileInput.addEventListener("change", () => {
        const node = current.folder;
        const chosen = [...el.fileInput.files];
        el.fileInput.value = "";
        if (!node || !chosen.length) return;

        const taken = new Set(node.files.map((item) => item.id));
        const added = chosen.map((picked) => {
            const item = file(picked.name, Date.now(), signedInUser, picked.size);
            item.id = uniqueSlug(item.name, taken);
            taken.add(item.id);
            item.isNew = true;
            return item;
        });
        node.files.push(...added);

        feedback = {
            message: `Uploaded ${plural(added.length, "file", "files")} to ${node.name}.`,
            undo: () => {
                node.files = node.files.filter((item) => !added.includes(item));
                feedback = { message: `Removed ${plural(added.length, "uploaded file", "uploaded files")}.` };
                pendingFocus = () => el.commands.querySelector("[data-action='upload']");
                go(folderUrl(node), { replace: true });
            },
        };
        pendingFocus = () => document.querySelector(`[data-focus-key="file:${CSS.escape(added[0].id)}"]`);
        render();
    });

    // ---------- Settings ----------

    function store(key, value) {
        try {
            localStorage.setItem(key, value);
        } catch {
            // Storage unavailable: the setting applies to this page view only.
        }
    }

    el.settings.addEventListener("change", (event) => {
        const input = event.target;
        if (input.name === "theme") {
            if (input.value === "system") delete html.dataset.theme;
            else html.dataset.theme = input.value;
            store("leonardo-sample-theme", input.value);
        }
        if (input.name === "density") {
            if (input.value === "standard") delete html.dataset.density;
            else html.dataset.density = input.value;
            store("leonardo-sample-density", input.value);
        }
    });

    el.settings.addEventListener("submit", (event) => event.preventDefault());

    for (const input of el.settings.querySelectorAll("input[name='theme']")) {
        input.checked = input.value === (html.dataset.theme ?? "system");
    }
    for (const input of el.settings.querySelectorAll("input[name='density']")) {
        input.checked = input.value === (html.dataset.density ?? "standard");
    }

    // ---------- Skip link ----------

    // The skip link must not change the hash, which holds the route.
    el.skipLink.addEventListener("click", (event) => {
        event.preventDefault();
        el.placeName.focus();
    });

    window.addEventListener("hashchange", render);
    render();
})();
