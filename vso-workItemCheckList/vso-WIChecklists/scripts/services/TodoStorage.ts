/// <reference path='../_all.ts' />

module todos {
    'use strict';

    export class todoStorage implements ITodoStorage {
        STORAGE_ID = 'todos-angularjs-typescript';

        workItemStorage(workItemId: number): string {
            return this.STORAGE_ID + workItemId;
        }

        get(workItemId: number): todoItem[] {
            return JSON.parse(localStorage.getItem(this.workItemStorage(workItemId)) || '[]');
        }

        put(todos: todoItem[], workItemId: number) {
            localStorage.setItem(this.workItemStorage(workItemId), JSON.stringify(todos));
        }
    } 
}