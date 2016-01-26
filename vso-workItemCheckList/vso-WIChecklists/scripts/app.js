var todos;
(function (todos) {
    'use strict';
    var todoItem = (function () {
        function todoItem(title, completed, workItemId) {
            this.title = title;
            this.completed = completed;
            this.workItemId = workItemId;
        }
        return todoItem;
    })();
    todos.todoItem = todoItem;
})(todos || (todos = {}));
var todos;
(function (todos) {
    'use strict';
    function todoFocus($timeout) {
        return {
            link: function ($scope, element, attributes) {
                $scope.$watch(attributes.todoFocus, function (newval) {
                    if (newval) {
                        $timeout(function () { return element[0].focus(); }, 0, false);
                    }
                });
            }
        };
    }
    todos.todoFocus = todoFocus;
    todoFocus.$inject = ['$timeout'];
})(todos || (todos = {}));
var todos;
(function (todos) {
    function todoBlur() {
        return {
            link: function ($scope, element, attributes) {
                element.bind('blur', function () { $scope.$apply(attributes.todoBlur); });
                $scope.$on('$destroy', function () { element.unbind('blur'); });
            }
        };
    }
    todos.todoBlur = todoBlur;
})(todos || (todos = {}));
var todos;
(function (todos) {
    'use strict';
    var ESCAPE_KEY = 27;
    function todoEscape() {
        return {
            link: function ($scope, element, attributes) {
                element.bind('keydown', function (event) {
                    if (event.keyCode === ESCAPE_KEY) {
                        $scope.$apply(attributes.todoEscape);
                    }
                });
                $scope.$on('$destroy', function () { element.unbind('keydown'); });
            }
        };
    }
    todos.todoEscape = todoEscape;
})(todos || (todos = {}));
var todos;
(function (todos_1) {
    'use strict';
    var todoStorage = (function () {
        function todoStorage() {
            this.STORAGE_ID = 'todos-angularjs-typescript';
        }
        todoStorage.prototype.workItemStorage = function (workItemId) {
            return this.STORAGE_ID + workItemId;
        };
        todoStorage.prototype.get = function (workItemId) {
            return JSON.parse(localStorage.getItem(this.workItemStorage(workItemId)) || '[]');
        };
        todoStorage.prototype.put = function (todos, workItemId) {
            localStorage.setItem(this.workItemStorage(workItemId), JSON.stringify(todos));
        };
        return todoStorage;
    })();
    todos_1.todoStorage = todoStorage;
})(todos || (todos = {}));
var todos;
(function (todos_2) {
    'use strict';
    var todoCtrl = (function () {
        function todoCtrl($scope, $location, todoStorage, filterFilter) {
            this.$scope = $scope;
            this.$location = $location;
            this.todoStorage = todoStorage;
            this.filterFilter = filterFilter;
            var self = this;
            VSS.require(['TFS/WorkItemTracking/Services'], function (workItemService) {
                workItemService.WorkItemFormService.getService().then(function (workItem) {
                    workItem.getId().then(function (workItemId) {
                        var todos = todoStorage.get(workItemId);
                        self.todos = todos;
                        self.workItemId = workItemId;
                        $scope.newTodo = '';
                        $scope.editedTodo = null;
                        $scope.vm = self;
                        $scope.todos = todos;
                        $scope.$apply();
                    });
                });
            });
            $scope.$watch('todos', function () { return self.onTodos(); }, true);
            $scope.$watch('location.path()', function (path) { return self.onPath(path); });
            if ($location.path() === '')
                $location.path('/');
            $scope.location = $location;
        }
        todoCtrl.prototype.onPath = function (path) {
            this.$scope.statusFilter = (path === '/active') ?
                { completed: false } : (path === '/completed') ?
                { completed: true } : {};
        };
        todoCtrl.prototype.onTodos = function () {
            this.$scope.remainingCount = this.filterFilter(this.todos, { completed: false }).length;
            this.$scope.doneCount = this.todos.length - this.$scope.remainingCount;
            this.$scope.allChecked = !this.$scope.remainingCount;
            this.todoStorage.put(this.todos, this.workItemId);
        };
        todoCtrl.prototype.addTodo = function () {
            var newTodo = this.$scope.newTodo.trim();
            if (!newTodo.length) {
                return;
            }
            ;
            this.todos.push(new todos_2.todoItem(newTodo, false, this.workItemId));
            this.$scope.newTodo = '';
        };
        todoCtrl.prototype.editTodo = function (todoItem) {
            this.$scope.editedTodo = todoItem;
            this.$scope.originalTodo = angular.extend({}, todoItem);
        };
        todoCtrl.prototype.revertEdits = function (todoItem) {
            this.todos[this.todos.indexOf(todoItem)] = this.$scope.originalTodo;
            this.$scope.reverted = true;
        };
        todoCtrl.prototype.doneEditing = function (todoItem) {
            this.$scope.editedTodo = null;
            this.$scope.originalTodo = null;
            if (this.$scope.reverted) {
                this.$scope.reverted = null;
                return;
            }
            todoItem.title = todoItem.title.trim();
            if (!todoItem.title)
                this.removeTodo(todoItem);
        };
        todoCtrl.prototype.removeTodo = function (todoItem) {
            this.todos.splice(this.todos.indexOf(todoItem), 1);
        };
        todoCtrl.prototype.clearDoneTodos = function () {
            this.$scope.todos = this.todos = this.todos.filter(function (todoItem) { return !todoItem.completed; });
        };
        todoCtrl.prototype.markAll = function (completed) {
            this.todos.forEach(function (todoItem) {
                todoItem.completed = completed;
            });
        };
        todoCtrl.$inject = [
            '$scope',
            '$location',
            'todoStorage',
            'filterFilter'
        ];
        return todoCtrl;
    })();
    todos_2.todoCtrl = todoCtrl;
})(todos || (todos = {}));
var todos;
(function (todos) {
    'use strict';
    var todomvc = angular.module('todomvc', []);
    todomvc.controller('todoCtrl', todos.todoCtrl);
    todomvc.directive('todoBlur', todos.todoBlur);
    todomvc.directive('todoFocus', todos.todoFocus);
    todomvc.directive('todoEscape', todos.todoEscape);
    todomvc.service('todoStorage', todos.todoStorage);
})(todos || (todos = {}));
var todos;
(function (todos) {
    function disableEnterSubmit() {
        return {
            link: function ($scope, element, attributes) {
                element.bind('keydown', function (event) {
                    if (event.keyCode == 13) {
                        event.preventDefault();
                        return false;
                    }
                    ;
                });
                $scope.$on('$destroy', function () { element.unbind('keydown'); });
            }
        };
    }
    todos.disableEnterSubmit = disableEnterSubmit;
})(todos || (todos = {}));
;
//# sourceMappingURL=app.js.map