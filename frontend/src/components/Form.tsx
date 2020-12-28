import React, { FC, useState, createContext, FormEvent } from 'react';
import { PrimaryButton, gray5, gray6 } from '../style/Styles';
/** @jsx jsx */
import { css, jsx } from '@emotion/core';

export interface Errors {
  [key: string]: string[];
}

export interface Touched {
  [key: string]: boolean;
}

export interface Values_Interface {
  //index
  [key: string]: any; //read string:any
}

// FORM CONTEXT --- SAME TO STATE ON EXPORT FORM ↓↓↓↓↓↓
interface FormContextProps {
  values_props: Values_Interface;
  setValue?: (fieldName: string, value: any) => void;
  errors: Errors;
  validate?: (fieldName: string) => void;
  touched: Touched;
  setTouched?: (fieldName: string) => void;
}

export const FormContext = createContext<FormContextProps>({
  values_props: {},
  errors: {},
  touched: {},
});

// FORM CONTEXT

// VALIDATIONS

type Validator = (value: any, args?: any) => string;

// VALIDATION NULL
export const required: Validator = (value: any): string =>
  value === undefined || value === null || value === ''
    ? 'This must be populated'
    : '';
// VALIDATION NULL

// VALIDATION MINLENGTH
export const minLength: Validator = (value: any, length: number): string =>
  value && value.length < length
    ? `This must be at least ${length} characters`
    : '';
// VALIDATION MINLENGTH

interface Validation {
  validator: Validator;
  arg?: any;
}

interface ValidationProp {
  [key: string]: Validation | Validation[];
}

// VALIDATIONS

export interface SubmitResult {
  success: boolean;
  errors?: Errors;
}

interface Props {
  submitCaption?: string;
  validationRules?: ValidationProp;
  onSubmit: (values: Values_Interface) => Promise<SubmitResult> | void;
  submitResult?: SubmitResult;
  successMessage?: string;
  failureMessage?: string;
}

export const Form: FC<Props> = ({
  submitCaption,
  children,
  validationRules,
  onSubmit,
  submitResult,
  successMessage = 'Success!',
  failureMessage = 'Something went wrong',
}) => {
  //STATE
  const [values_props, setValues] = useState<Values_Interface>({});
  const [errors, setErrors] = useState<Errors>({});
  const [touched, setTouched] = useState<Touched>({});
  const [submitting, setSubmitting] = useState(false);
  const [submitted, setSubmitted] = useState(false);
  const [submitError, setSubmitError] = useState(false);

  const validate = (fieldName: string): string[] => {
    if (!validationRules) {
      return [];
    }
    if (!validationRules[fieldName]) {
      return [];
    }
    const rules = Array.isArray(validationRules[fieldName])
      ? (validationRules[fieldName] as Validation[])
      : ([validationRules[fieldName]] as Validation[]);

    const fieldErrors: string[] = [];
    rules.forEach((rule) => {
      const error = rule.validator(values_props[fieldName], rule.arg);
      if (error) {
        fieldErrors.push(error);
      }
    });

    const newErrors = { ...errors, [fieldName]: fieldErrors };
    setErrors(newErrors);
    return fieldErrors;
  };

  const handleSubmit = async (e: FormEvent<HTMLFormElement>) => {
    e.preventDefault();
    if (validateForm()) {
      setSubmitting(true);
      setSubmitError(false);
      const result = await onSubmit(values_props);

      // The result may be passed through as a prop
      if (result === undefined) {
        return;
      }

      setErrors(result.errors || {});
      setSubmitError(!result.success);
      setSubmitting(false);
      setSubmitted(true);

      // TODO - set state to indicate submission is in progress
      // TODO - call the consumer submit function
      // TODO - set any errors in state
      // TODO - set state to indicate submission has finished
    }
  };

  const validateForm = () => {
    const newErrors: Errors = {};
    let haveError: boolean = false;
    if (validationRules) {
      Object.keys(validationRules).forEach((fieldName) => {
        newErrors[fieldName] = validate(fieldName);
        if (newErrors[fieldName].length > 0) {
          haveError = true;
        }
      });
    }
    setErrors(newErrors);
    return !haveError;
  };

  const disabled = submitResult
    ? submitResult.success
    : submitting || (submitted && !submitError);
  const showError = submitResult
    ? !submitResult.success
    : submitted && submitError;

  const showSuccess = submitResult
    ? submitResult.success
    : submitted && !submitError;

  return (
    //Same FORMCONTEXT INTERFACE
    <FormContext.Provider
      value={{
        values_props,
        setValue: (fieldName: string, value: any) => {
          setValues({ ...values_props, [fieldName]: value });
        },
        errors,
        validate,
        touched,
        setTouched: (fieldName: string) => {
          setTouched({ ...touched, [fieldName]: true });
        },
      }}
    >
      <form noValidate={true} onSubmit={handleSubmit}>
        <fieldset
          disabled={disabled}
          css={css`
            margin: 10px auto 0 auto;
            padding: 30px;
            width: 350px;
            background-color: ${gray6};
            border-radius: 4px;
            border: 1px solid ${gray5};
            box-shadow: 0 3px 5px 0 rgba(0, 0, 0, 0.16);
          `}
        >
          {children}
          <div
            css={css`
              margin: 30px 0px 0px 0px;
              padding: 20px 0px 0px 0px;
              border-top: 1px solid ${gray5};
            `}
          >
            <PrimaryButton type="submit">{submitCaption}</PrimaryButton>
          </div>
          {showError && (
            <p
              css={css`
                color: red;
              `}
            >
              {failureMessage}
            </p>
          )}
          {showSuccess && (
            <p
              css={css`
                color: green;
              `}
            >
              {successMessage}
            </p>
          )}
        </fieldset>
      </form>
    </FormContext.Provider>
  );
};

// export default Form;
