﻿using UnityEngine;
using System.Text;
using System.Collections;
using System.Collections.Generic;

namespace iCanScript.Editor.CodeEngineering {

    public class ExecutionBlockDefinition : CodeBase {
        // ===================================================================
        // FIELDS
        // -------------------------------------------------------------------
        protected List<CodeBase>    myExecutionList= new List<CodeBase>();

        // ===================================================================
        // INFORMATION GATHERING FUNCTIONS
        // -------------------------------------------------------------------
        /// Builds an execution code block.
        ///
        /// @param vsObject The visual script objects associated with this code.
        /// @param parent   The code block parent.
        /// @return The newly created code context.
        ///
        public ExecutionBlockDefinition(iCS_EditorObject vsObject, CodeBase parent)
        : base(vsObject, parent) {
        }

        // ===================================================================
        // COMMON INTERFACE FUNCTIONS
        // -------------------------------------------------------------------
		/// Resolves code dependencies.
		public override void ResolveDependencies() {
			foreach(var e in myExecutionList.ToArray()) {
				e.ResolveDependencies();
			}
		}

        // -------------------------------------------------------------------
        /// Adds an execution child.
        ///
        /// @param child The execution child to add.
        ///
        public override void AddExecutable(CodeBase child) {
            myExecutionList.Add(child);
            child.Parent= this;
        }

        // -------------------------------------------------------------------
        /// Removes a code context from the function.
        ///
        /// @param toRemove The code context to be removed.
        ///
        public override void Remove(CodeBase toRemove) {
            if(myExecutionList.Remove(toRemove)) {
                toRemove.Parent= null;
            }
        }
                
        // ===================================================================
        // CODE GENERATION FUNCTIONS
        // -------------------------------------------------------------------
        /// Generate the execution list code.
        ///
        /// @param indentSize The indentation needed for the class definition.
        /// @return The formatted body code for the if-statement.
        ///
        public override string GenerateBody(int indentSize) {
            var result= new StringBuilder(1024);
            foreach(var c in myExecutionList) {
                result.Append(c.GenerateCode(indentSize));
            }
            return result.ToString();
        }
        
    }

}