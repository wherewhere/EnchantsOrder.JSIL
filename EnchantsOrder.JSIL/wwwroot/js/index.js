"use strict";

function runMain() {
    var asm = JSIL.GetAssembly("EnchantsOrder.JSIL", true);

    System.Int64.prototype[new JSIL.MethodSignature(System.Int32, [System.Int64]).GetNamedKey("CompareTo", true)] = function (value) {
        if (System.Int64.op_LessThan(this, value)) { return -1; }
        if (System.Int64.op_GreaterThan(this, value)) { return 1; }
        return 0;
    };

    System.Threading.Tasks.Task.prototype.then = function (onFulfilled, onRejected) {
        new JSIL.MethodSignature(System.Threading.Tasks.Task, [System.Action$b1.Of(System.Threading.Tasks.Task)]).CallVirtual("ContinueWith", null, this, function (t) {
            if (t.IsFaulted) {
                if (onRejected) {
                    onRejected(t.Exception);
                }
            }
            else {
                if (onFulfilled) {
                    onFulfilled();
                }
            }
        });
    };
    System.Threading.Tasks.Task$b1.prototype.then = function (onFulfilled, onRejected) {
        new JSIL.MethodSignature(System.Threading.Tasks.Task, [System.Action$b1.Of(System.Threading.Tasks.Task)]).CallVirtual("ContinueWith", null, this, function (t) {
            if (t.IsFaulted) {
                if (onRejected) {
                    onRejected(t.Exception);
                }
            }
            else {
                if (onFulfilled) {
                    onFulfilled(t.Result);
                }
            }
        });
    };

    Object.defineProperty(System.Linq.Enumerable, new JSIL.MethodSignature(System.Int32, [System.Collections.Generic.IEnumerable$b1.Of(System.Int32)]).GetNamedKey("Min", true), {
        get: function () {
            return function (source) {
                var value;
                var e = System.Collections.Generic.IEnumerable$b1.Of(System.Int32).GetEnumerator.Call(source, null);
                try {
                    if (!e.MoveNext()) {
                        throw new System.Exception("NoElementsException");
                    }
                    value = e.get_Current();
                    while (e.MoveNext()) {
                        var x = e.get_Current();
                        if (x < value) {
                            value = x;
                        }
                    }
                }
                finally {
                    if (e !== null) {
                        e.Dispose();
                    }
                }
                return value;
            };
        }
    });

    Object.defineProperty(System.Linq.Enumerable, new JSIL.MethodSignature(System.Int64, [System.Collections.Generic.IEnumerable$b1.Of(System.Int64)]).GetNamedKey("Max", true), {
        get: function () {
            return function (source) {
                var value;
                var e = System.Collections.Generic.IEnumerable$b1.Of(System.Int64).GetEnumerator.Call(source, null);
                try {
                    if (!e.MoveNext()) {
                        throw new System.Exception("NoElementsException");
                    }
                    value = e.get_Current();
                    while (e.MoveNext()) {
                        var x = e.get_Current();
                        if (System.Int64.op_GreaterThan(x, value)) {
                            value = x;
                        }
                    }
                }
                finally {
                    if (e !== null) {
                        e.Dispose();
                    }
                }
                return value;
            };
        }
    });

    var appends = [
        new JSIL.MethodSignature(System.Text.StringBuilder, [System.Char]).GetNamedKey("Append", true),
        new JSIL.MethodSignature(System.Text.StringBuilder, [System.Int32]).GetNamedKey("Append", true),
        new JSIL.MethodSignature(System.Text.StringBuilder, [System.String]).GetNamedKey("Append", true),
        new JSIL.MethodSignature(System.Text.StringBuilder, [System.Object]).GetNamedKey("Append", true),
    ];
    var originalAppends = {};
    for (var i = 0; i < appends.length; i++) {
        var name = appends[i];
        originalAppends[name] = System.Text.StringBuilder.prototype[name];
        Object.defineProperty(System.Text.StringBuilder.prototype, name, {
            get: function () {
                return function (value) {
                    return originalAppends[name].call(this, value) || this;
                };
            }
        });
    }

    if (typeof Promise !== "undefined") {
        Promise.prototype.GetAwaiter = function () {
            return ((new (EnchantsOrder.JSIL.Common.Promise$b1.Of(System.Object))(this))).GetAwaiter();
        }
    }

    JSIL.InvokeEntryPoint(asm, []);
};